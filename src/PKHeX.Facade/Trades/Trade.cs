using PKHeX.Core;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Trades;

/// <summary>
/// Moves Pokémon between the loaded save and a partner save, the way link trades, the Time Capsule, Pal Park and Poké Transfer do.
/// </summary>
public class Trade(Game mine, Game partner)
{
    public Game Mine { get; } = mine;
    public Game Partner { get; } = partner;

    public TradeRoute? SendRoute => RouteBetween(Mine, Partner);
    public TradeRoute? ReceiveRoute => RouteBetween(Partner, Mine);

    public TradeRoom Room => new(EmptyBoxSlots(Mine).Count, EmptyBoxSlots(Partner).Count);

    public static TradeRoute? RouteBetween(Game from, Game to)
    {
        if (from.Format is not null || to.Format is not null) return null;

        var (source, target) = (from.SaveFile.PKMType, to.SaveFile.PKMType);
        // Let's Go keeps its party in box storage, and taking a member out of it isn't handled.
        if (source == typeof(PB7) || target == typeof(PB7)) return null;
        if (source == target) return TradeRoute.Link;
        if (IsGameBoy(source) && IsGameBoy(target)) return TradeRoute.TimeCapsule;
        if (source == typeof(PK3) && target == typeof(PK4)) return TradeRoute.PalPark;
        if (source == typeof(PK4) && target == typeof(PK5)) return TradeRoute.PokeTransfer;
        return null;
    }

    public TradePreview Preview(TradeOffer offer)
    {
        var offers = new List<TradedPokemon>();
        var refused = new List<RefusedPokemon>();
        Plan(TradeDirection.Send, offer.Send, offers, refused);
        Plan(TradeDirection.Receive, offer.Receive, offers, refused);
        return new TradePreview(offers, refused);
    }

    /// <exception cref="TradeRefusedException">A Pokémon in the offer is refused. Neither save changes.</exception>
    public IReadOnlyList<TradeArrival> Commit(TradeOffer offer)
    {
        var preview = Preview(offer);
        if (preview.Refused.Count > 0) throw new TradeRefusedException(preview.Refused);

        Mine.Trainer.Commit();
        Partner.Trainer.Commit();

        foreach (var sent in preview.Offers.GroupBy(o => o.Direction))
            Remove(From(sent.Key), sent.Select(o => o.From).ToList());

        foreach (var arrived in preview.Offers.GroupBy(o => o.Direction))
            To(arrived.Key).Trainer.PokemonBox.Place(arrived.Select(o => (o.Arrives.Pkm, o.ArrivesAt)));

        return preview.Offers
            .Select(o => new TradeArrival(o.Direction, o.ArrivesAt, To(o.Direction).Trainer.PokemonBox.All[o.ArrivesAt]))
            .ToList();
    }

    public Game From(TradeDirection direction) => direction == TradeDirection.Send ? Mine : Partner;
    public Game To(TradeDirection direction) => direction == TradeDirection.Send ? Partner : Mine;

    private void Plan(TradeDirection direction, IEnumerable<TradeSlot> slots, List<TradedPokemon> offers, List<RefusedPokemon> refused)
    {
        var (from, to) = (From(direction), To(direction));
        var route = RouteBetween(from, to);
        var room = new Queue<int>(EmptyBoxSlots(to));
        var partyLeft = from.Trainer.Party.Pokemons.Count(p => !p.Pkm.IsEgg);
        var caught = new HashSet<Species>();

        foreach (var slot in slots.Distinct())
        {
            var pokemon = PokemonAt(from, slot);
            var leavesParty = slot.Source == PokemonSource.Party && !pokemon.Pkm.IsEgg;
            Pokemon? arrives = null;
            var refusal = route switch
            {
                null => TradeRefusal.NoRoute,
                _ when slot.Source == PokemonSource.Box && from.SaveFile.IsBoxSlotOverwriteProtected(slot.Index) => TradeRefusal.SlotLocked,
                not TradeRoute.Link when pokemon.Pkm.IsEgg => TradeRefusal.EggAcrossGenerations,
                _ when !IsLanguageCompatible(pokemon, to) => TradeRefusal.LanguageMismatch,
                _ when Convert(pokemon, to, out arrives) is { } failed => failed,
                _ when leavesParty && partyLeft == 1 => TradeRefusal.LastPartyMember,
                _ when room.Count == 0 => TradeRefusal.NoRoom,
                _ => (TradeRefusal?)null,
            };

            if (refusal is { } reason)
            {
                refused.Add(new RefusedPokemon(direction, slot, reason));
                continue;
            }

            if (leavesParty) partyLeft--;
            var saveChanges = !arrives!.Pkm.IsEgg && to.SaveFile.HasPokeDex && !to.SaveFile.GetCaught(arrives.Pkm.Species) && caught.Add(arrives.Species.Species)
                ? [new TradeSaveChange(TradeSaveChangeKind.PokedexCaught, arrives.Species)]
                : Array.Empty<TradeSaveChange>();
            offers.Add(new TradedPokemon(
                direction,
                slot,
                pokemon,
                room.Dequeue(),
                arrives,
                Changes(pokemon, arrives, route!.Value),
                saveChanges,
                LegalityIn(to, arrives)));
        }
    }

    private static Pokemon PokemonAt(Game game, TradeSlot slot)
    {
        var pokemons = slot.Source == PokemonSource.Party ? game.Trainer.Party.Pokemons : game.Trainer.PokemonBox.All;
        if (slot.Index < 0 || slot.Index >= pokemons.Count || pokemons[slot.Index].IsEmpty)
            throw new ArgumentOutOfRangeException(nameof(slot), $"There is no Pokémon in {slot.Source} slot {slot.Index}.");

        return pokemons[slot.Index];
    }

    private static List<int> EmptyBoxSlots(Game game)
    {
        var all = game.Trainer.PokemonBox.All;
        return Enumerable.Range(0, all.Count)
            .Where(index => all[index].IsEmpty && !game.SaveFile.IsBoxSlotOverwriteProtected(index))
            .ToList();
    }

    private static bool IsGameBoy(Type type) => type == typeof(PK1) || type == typeof(PK2);

    // ConvertToType doesn't check it, so a Japanese Gen 1 or 2 Pokémon would convert into an international save.
    private static bool IsLanguageCompatible(Pokemon pokemon, Game to) =>
        to.SaveFile is not ILangDeviantSave save || EntityConverter.IsCompatibleGB(to.SaveFile.BlankPKM, save.Japanese, pokemon.Pkm.Japanese);

    private static TradeRefusal? Convert(Pokemon pokemon, Game to, out Pokemon? arrives)
    {
        arrives = null;
        var converted = EntityConverter.ConvertToType(pokemon.Pkm.Clone(), to.SaveFile.PKMType, out var result);
        if (converted is null)
            return result is EntityConverterResult.IncompatibleSpecies or EntityConverterResult.IncompatibleForm
                ? TradeRefusal.SpeciesNotInGame
                : TradeRefusal.NoRoute;

        to.SaveFile.AdaptToSaveFile(converted, isParty: false);
        arrives = new Pokemon(converted, to);
        return to.IsAwareOf(arrives) ? null : TradeRefusal.SpeciesNotInGame;
    }

    private static void Remove(Game game, IReadOnlyList<TradeSlot> slots)
    {
        var party = slots.Where(s => s.Source == PokemonSource.Party).Select(s => s.Index).ToList();
        if (party.Count > 0) game.Trainer.Party.Remove(party);

        var box = slots.Where(s => s.Source == PokemonSource.Box).Select(s => s.Index).ToList();
        if (box.Count > 0) game.Trainer.PokemonBox.Remove(box);
    }

    private static IReadOnlyList<TradeChange> Changes(Pokemon from, Pokemon arrives, TradeRoute route)
    {
        var reason = route switch
        {
            TradeRoute.Link => TradeChangeReason.Link,
            TradeRoute.TimeCapsule => TradeChangeReason.TimeCapsule,
            TradeRoute.PalPark => TradeChangeReason.PalPark,
            _ => TradeChangeReason.PokeTransfer,
        };
        var changes = new List<TradeChange>();

        void Compare(TradeField field, string? before, string? after, TradeChangeReason why)
        {
            if (before != after) changes.Add(new TradeChange(field, before, after, why));
        }

        Compare(TradeField.Species, from.Species.Name, arrives.Species.Name, reason);
        Compare(TradeField.Form, from.Form.Form.Name, arrives.Form.Form.Name, reason);
        var item = HeldItem(arrives);
        Compare(TradeField.HeldItem, HeldItem(from), item, item is null ? TradeChangeReason.ItemRemoved : TradeChangeReason.ItemRemapped);

        var (movesBefore, movesAfter) = (Moves(from), Moves(arrives));
        // Pal Park and Poké Transfer only take away HM moves.
        var forgotten = route is TradeRoute.PalPark or TradeRoute.PokeTransfer ? TradeChangeReason.HmRemoved : reason;
        changes.AddRange(movesBefore.Except(movesAfter).Select(move => new TradeChange(TradeField.Moves, move, null, forgotten)));
        changes.AddRange(movesAfter.Except(movesBefore).Select(move => new TradeChange(TradeField.Moves, null, move, reason)));

        Compare(TradeField.MetLocation, MetLocation(from), MetLocation(arrives), reason);
        Compare(TradeField.MetLevel, MetLevel(from), MetLevel(arrives), reason);
        Compare(TradeField.Ball, from.Ball.Name, arrives.Ball.Name, reason);
        Compare(TradeField.Friendship, from.Pkm is PK1 ? null : from.Friendship.ToString(), arrives.Pkm is PK1 ? null : arrives.Friendship.ToString(), reason);
        Compare(TradeField.Nickname, from.Nickname, arrives.Nickname, reason);
        Compare(TradeField.Ability, from.Ability.Name, arrives.Ability.Name, reason);
        return changes;
    }

    // Gen 1 stores no met data and Gen 2 only for Pokémon caught in Crystal, which leaves the fields at 0.
    private static bool HasNoMetData(Pokemon pokemon) => pokemon.Pkm.Format <= 2 && pokemon.MetConditions.Location.Id == 0 && pokemon.MetConditions.Level == 0;

    // PKHeX can't name the met location of a Pokémon from an earlier generation, such as Pal Park on a Gen 3 Pokémon in Gen 4.
    private static string? MetLocation(Pokemon pokemon) => HasNoMetData(pokemon)
        ? null
        : pokemon.Game.LocationRepository.GetBy(pokemon.MetConditions.Location.Id) is { } location && location != LocationDefinition.Unknown
            ? location.Name
            : pokemon.MetConditions.Location.Name;

    private static string? MetLevel(Pokemon pokemon) => HasNoMetData(pokemon) ? null : pokemon.MetConditions.Level.ToString();

    private static string? HeldItem(Pokemon pokemon) => pokemon.HeldItem is { IsNone: false } item ? item.Name : null;

    private static List<string> Moves(Pokemon pokemon) => pokemon.Moves.Values
        .Select(slot => slot.Move)
        .Where(move => move.Id != 0)
        .Select(move => move.Name)
        .ToList();

    // ParseSettings is global. Gen 1 and 2 need the destination's cartridge rules, and nothing else sets them, so the defaults come back after.
    private static PokemonLegality LegalityIn(Game destination, Pokemon arrival)
    {
        ParseSettings.InitFromSaveFileData(destination.SaveFile);
        try
        {
            return arrival.LegalityReport();
        }
        finally
        {
            ParseSettings.ClearActiveTrainer();
            ParseSettings.AllowEraCartGB = false;
            ParseSettings.AllowEraCartGBA = true;
            ParseSettings.AllowEraSwitchGBA = false;
        }
    }
}
