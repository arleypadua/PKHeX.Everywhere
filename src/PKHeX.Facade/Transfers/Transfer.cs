using PKHeX.Core;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Transfers;

/// <summary>
/// Moves Pokémon between the loaded save and a partner save, the way link trades, the Time Capsule, Pal Park and Poké Transfer do.
/// </summary>
public class Transfer(Game mine, Game partner)
{
    public Game Mine { get; } = mine;
    public Game Partner { get; } = partner;

    public TransferRoute? SendRoute => RouteBetween(Mine, Partner);
    public TransferRoute? ReceiveRoute => RouteBetween(Partner, Mine);

    public TransferRoom Room => new(EmptyBoxSlots(Mine).Count, EmptyBoxSlots(Partner).Count);

    public Game SaveOf(TransferSave save) => save == TransferSave.Mine ? Mine : Partner;

    public static TransferRoute? RouteBetween(Game from, Game to)
    {
        if (!HasRoutes(from) || !HasRoutes(to)) return null;
        return OfficialRoute(from.SaveFile.PKMType, to.SaveFile.PKMType) ?? TransferRoute.Unofficial;
    }

    // Let's Go keeps its party in box storage, and taking a member out of it isn't handled.
    private static bool HasRoutes(Game game) => game.Format is null && game.SaveFile.PKMType != typeof(PB7);

    /// <summary>
    /// Converts a Pokémon from a file into <paramref name="to"/> as a transfer would, writing nothing.
    /// It takes the unofficial route only when PKHeX has no conversion of its own.
    /// </summary>
    /// <exception cref="PokemonRefusedException">The Pokémon can't be moved into the save.</exception>
    public static PokemonImport Import(Pokemon pokemon, Game to)
    {
        var refusal = TransferConversion.Convert(pokemon, to, unofficial: false, out var arrives);
        var unofficial = refusal == TransferRefusal.ConversionFailed;
        if (unofficial)
        {
            refusal = !HasRoutes(to) ? TransferRefusal.ConversionFailed
                : pokemon.Pkm.IsEgg ? TransferRefusal.EggAcrossGenerations
                : !IsLanguageCompatible(pokemon, to) ? TransferRefusal.LanguageMismatch
                : TransferConversion.Convert(pokemon, to, unofficial: true, out arrives);
        }

        if (refusal is { } reason) throw new PokemonRefusedException(reason);

        var route = OfficialRoute(pokemon.Pkm.GetType(), to.SaveFile.PKMType) ?? TransferRoute.Unofficial;
        return new PokemonImport(arrives!, unofficial, Changes(pokemon, arrives!, route).OrderBy(change => change.Field).ToList());
    }

    /// <summary>
    /// Converts a Pokémon to <paramref name="to"/>'s format as <see cref="Import"/> does, writing nothing. The same Pokémon always converts to the same bytes.
    /// </summary>
    /// <exception cref="PokemonRefusedException">The Pokémon can't be moved into the save.</exception>
    public PokemonImport Convert(Pokemon pokemon, TransferSave to) => Import(pokemon, SaveOf(to));

    private static TransferRoute? OfficialRoute(Type source, Type target)
    {
        if (source == target) return TransferRoute.Link;
        if (IsGameBoy(source) && IsGameBoy(target)) return TransferRoute.TimeCapsule;
        if (source == typeof(PK3) && target == typeof(PK4)) return TransferRoute.PalPark;
        if (source == typeof(PK4) && target == typeof(PK5)) return TransferRoute.PokeTransfer;
        return null;
    }

    /// <exception cref="ArgumentException">An arrival is for a Pokémon the offer doesn't have.</exception>
    /// <exception cref="UnreadablePokemonException">An arrival's bytes aren't a Pokémon of the destination's format.</exception>
    /// <exception cref="InvalidPatchException">An arrival's patch holds a value the destination can't store.</exception>
    public TransferPreview Preview(TransferOffer offer)
    {
        var arrivals = offer.Arrivals ?? [];
        if (arrivals.FirstOrDefault(a => !(a.Direction == TransferDirection.Send ? offer.Send : offer.Receive).Contains(a.At)) is { } stray)
            throw new ArgumentException($"There is no offered Pokémon in {stray.At.Source} slot {stray.At.Index} to arrive as given.", nameof(offer));

        var offers = new List<TransferredPokemon>();
        var refused = new List<RefusedPokemon>();
        Plan(TransferDirection.Send, offer.Send, arrivals, offers, refused);
        Plan(TransferDirection.Receive, offer.Receive, arrivals, offers, refused);
        return new TransferPreview(offers, refused);
    }

    /// <exception cref="TransferRefusedException">A Pokémon in the offer is refused. Neither save changes.</exception>
    /// <exception cref="ArgumentException">An arrival is for a Pokémon the offer doesn't have.</exception>
    /// <exception cref="UnreadablePokemonException">An arrival's bytes aren't a Pokémon of the destination's format.</exception>
    /// <exception cref="InvalidPatchException">An arrival's patch holds a value the destination can't store.</exception>
    public IReadOnlyList<TransferArrival> Commit(TransferOffer offer)
    {
        var preview = Preview(offer);
        if (preview.Refused.Count > 0) throw new TransferRefusedException(preview.Refused);

        Mine.Trainer.Commit();
        Partner.Trainer.Commit();

        foreach (var sent in preview.Offers.GroupBy(o => o.Direction))
            Remove(From(sent.Key), sent.Select(o => o.From).ToList());

        foreach (var arrived in preview.Offers.GroupBy(o => o.Direction))
            To(arrived.Key).Trainer.PokemonBox.Place(arrived.Select(o => (o.Arrives.Pkm, o.ArrivesAt)));

        foreach (var offered in preview.Offers)
            Apply(offered);

        return preview.Offers
            .Select(o => new TransferArrival(o.Direction, o.ArrivesAt, To(o.Direction).Trainer.PokemonBox.All[o.ArrivesAt]))
            .ToList();
    }

    public Game From(TransferDirection direction) => direction == TransferDirection.Send ? Mine : Partner;
    public Game To(TransferDirection direction) => direction == TransferDirection.Send ? Partner : Mine;

    private void Plan(TransferDirection direction, IEnumerable<TransferSlot> slots, IReadOnlyList<ArrivalOverride> arrivals, List<TransferredPokemon> offers, List<RefusedPokemon> refused)
    {
        var (from, to) = (From(direction), To(direction));
        var route = RouteBetween(from, to);
        var room = new Queue<int>(EmptyBoxSlots(to));
        var partyLeft = from.Trainer.Party.Pokemons.Count(p => !p.Pkm.IsEgg);
        var caught = new HashSet<Species>();
        var arceusEventStarted = false;
        var orbsReturned = 0;

        foreach (var slot in slots.Distinct())
        {
            var pokemon = PokemonAt(from, slot);
            var leavesParty = slot.Source == PokemonSource.Party && !pokemon.Pkm.IsEgg;
            var leaving = pokemon.Clone();
            var tookOrb = route == TransferRoute.Link && from.SaveFile is SAV4Pt && TransferEffects.RevertPlatinumFormTakingOrb(leaving.Pkm);
            Pokemon? converted = null;
            var refusal = route switch
            {
                null => TransferRefusal.NoRoute,
                _ when slot.Source == PokemonSource.Box && from.SaveFile.IsBoxSlotOverwriteProtected(slot.Index) => TransferRefusal.SlotLocked,
                not TransferRoute.Link when pokemon.Pkm.IsEgg => TransferRefusal.EggAcrossGenerations,
                _ when !IsLanguageCompatible(pokemon, to) => TransferRefusal.LanguageMismatch,
                _ when TransferConversion.Convert(leaving, to, route == TransferRoute.Unofficial, out converted) is { } failed => failed,
                _ when leavesParty && partyLeft == 1 => TransferRefusal.LastPartyMember,
                _ when room.Count == 0 => TransferRefusal.NoRoom,
                _ when tookOrb && !BagHasRoom(from, TransferEffects.GriseousOrb, orbsReturned + 1) => TransferRefusal.BagFull,
                _ => (TransferRefusal?)null,
            };

            if (refusal is { } reason)
            {
                refused.Add(new RefusedPokemon(direction, slot, reason));
                continue;
            }

            if (leavesParty) partyLeft--;
            if (tookOrb) orbsReturned++;
            var received = converted!.Clone();
            TransferEffects.Receive(received.Pkm, route!.Value);
            var evolved = received.Clone();
            if (!IsStadium(from) && !IsStadium(to)) TransferEffects.Evolve(evolved.Pkm, to, route.Value);
            var given = arrivals.FirstOrDefault(a => a.Direction == direction && a.At == slot);
            var arrives = given is null ? evolved : Arrival(given, to);

            var changes = (given is null
                    ? Changes(pokemon, leaving, TransferChangeReason.FormReverted)
                        .Concat(Changes(leaving, converted, route.Value))
                        .Concat(Changes(converted, received, TransferChangeReason.Received))
                        .Concat(Changes(received, evolved, TransferChangeReason.TradeEvolution, TransferChangeReason.ItemUsed))
                    : Changes(pokemon, arrives, route.Value))
                .OrderBy(change => change.Field)
                .ToList();

            var saveChanges = new List<TransferSaveChange>();
            if (tookOrb) saveChanges.Add(new TransferSaveChange(TransferSaveChangeKind.ItemReturned, TransferSide.Sender, from.ItemRepository.GetGameItem(TransferEffects.GriseousOrb).Name));
            if (!arrives.Pkm.IsEgg && to.SaveFile.HasPokeDex && !to.SaveFile.GetCaught(arrives.Pkm.Species) && caught.Add(arrives.Species.Species))
                saveChanges.Add(new TransferSaveChange(TransferSaveChangeKind.PokedexCaught, TransferSide.Receiver, arrives.Species.Name));
            if (!arceusEventStarted && TransferEffects.StartsArceusEvent(arrives.Pkm, to))
            {
                arceusEventStarted = true;
                saveChanges.Add(new TransferSaveChange(TransferSaveChangeKind.EventVar, TransferSide.Receiver, ArceusEventLabel(to)));
            }

            offers.Add(new TransferredPokemon(
                direction,
                route.Value,
                slot,
                pokemon,
                room.Dequeue(),
                arrives,
                changes,
                saveChanges,
                LegalityIn(to, arrives)));
        }
    }

    private void Apply(TransferredPokemon offered)
    {
        foreach (var change in offered.SaveChanges)
        {
            switch (change.Kind)
            {
                case TransferSaveChangeKind.ItemReturned:
                    ReturnToBag(From(offered.Direction), TransferEffects.GriseousOrb);
                    break;
                case TransferSaveChangeKind.EventVar:
                    ((SAV4Pt)To(offered.Direction).SaveFile).SetWork(TransferEffects.ArceusEventWork, 1);
                    break;
            }
        }
    }

    // pret/pokeplatinum ScrCmd_TryRevertPartyPokemonForms refuses the trade when the bag can't take the orb back.
    private static bool BagHasRoom(Game game, ushort item, int count)
    {
        if (PouchFor(game, item) is not { } pouch) return false;
        var owned = pouch.Items.FirstOrDefault(owned => owned.Id == item);
        return owned is null ? pouch.Items.Any(slot => slot.IsNone) && count <= pouch.MaxCountOf(item) : owned.Count + count <= pouch.MaxCountOf(item);
    }

    private static void ReturnToBag(Game game, ushort item)
    {
        var pouch = PouchFor(game, item)!;
        var owned = pouch.Items.FirstOrDefault(owned => owned.Id == item)?.Count ?? 0;
        pouch.TrySet(item, (uint)owned + 1);
    }

    private static Inventory? PouchFor(Game game, ushort item) =>
        game.Trainer.Inventories.InventoryItems.Values.FirstOrDefault(pouch => pouch.Supports(game.ItemRepository.GetGameItem(item)));


    private static string ArceusEventLabel(Game game)
    {
        var work = game.Events?.Work.FirstOrDefault(work => work.Index == TransferEffects.ArceusEventWork);
        var state = work?.Options.FirstOrDefault(option => option.Value == 1);
        return work is null || state is null ? "Arceus event" : $"{work.Name}: {state.Name}";
    }

    private static Pokemon PokemonAt(Game game, TransferSlot slot)
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

    private static bool IsGameBoy(Type type) => type == typeof(PK1) || type == typeof(PK2) || type == typeof(SK2);

    // Storing a Pokémon in a Stadium isn't a trade, so nothing evolves on the way in or out.
    private static bool IsStadium(Game game) => game.SaveFile is SAV_STADIUM;

    // ConvertToType doesn't check it, so a Japanese Gen 1 or 2 Pokémon would convert into an international save.
    private static bool IsLanguageCompatible(Pokemon pokemon, Game to) =>
        to.SaveFile is not ILangDeviantSave save || EntityConverter.IsCompatibleGB(to.SaveFile.BlankPKM, save.Japanese, pokemon.Pkm.Japanese);

    private static void Remove(Game game, IReadOnlyList<TransferSlot> slots)
    {
        var party = slots.Where(s => s.Source == PokemonSource.Party).Select(s => s.Index).ToList();
        if (party.Count > 0) game.Trainer.Party.Remove(party);

        var box = slots.Where(s => s.Source == PokemonSource.Box).Select(s => s.Index).ToList();
        if (box.Count > 0) game.Trainer.PokemonBox.Remove(box);
    }

    private static Pokemon Arrival(ArrivalOverride given, Game to)
    {
        var file = Pokemon.ReadFile(given.Bytes, to.SaveFile.Generation);
        if (file.Pkm.GetType() != to.SaveFile.PKMType)
            throw new UnreadablePokemonException(UnreadableReason.UnsupportedFormat, $"The arrival is a {file.Pkm.GetType().Name}, not a {to.SaveFile.PKMType.Name}.");

        var arrives = new Pokemon(file.Pkm, to);
        arrives.Update(given.Patch);
        return arrives;
    }

    private static IEnumerable<TransferChange> Changes(Pokemon from, Pokemon arrives, TransferRoute route)
    {
        if (route == TransferRoute.Unofficial)
        {
            return Changes(from, arrives, (field, before, after) => field switch
            {
                TransferField.Moves or TransferField.HeldItem when after is null => TransferChangeReason.NotInGame,
                TransferField.HeldItem => TransferChangeReason.ItemRemapped,
                TransferField.Ball when before is not null && !TransferConversion.HasBall(arrives.Game, from.Pkm.Ball) => TransferChangeReason.NotInGame,
                TransferField.Ability when before is not null && !TransferConversion.CanHaveAbility(arrives.Pkm, from.Pkm.Ability) => TransferChangeReason.NotInGame,
                _ => TransferChangeReason.Unofficial,
            });
        }

        var reason = route switch
        {
            TransferRoute.Link => TransferChangeReason.Link,
            TransferRoute.TimeCapsule => TransferChangeReason.TimeCapsule,
            TransferRoute.PalPark => TransferChangeReason.PalPark,
            _ => TransferChangeReason.PokeTransfer,
        };
        return Changes(from, arrives, (field, _, after) => field switch
        {
            TransferField.HeldItem => after is null ? TransferChangeReason.ItemRemoved : TransferChangeReason.ItemRemapped,
            // Pal Park and Poké Transfer only take away HM moves.
            TransferField.Moves when after is null && route is TransferRoute.PalPark or TransferRoute.PokeTransfer => TransferChangeReason.HmRemoved,
            _ => reason,
        });
    }

    private static IEnumerable<TransferChange> Changes(Pokemon from, Pokemon arrives, TransferChangeReason reason, TransferChangeReason? itemReason = null) =>
        Changes(from, arrives, (field, _, _) => field == TransferField.HeldItem ? itemReason ?? reason : reason);

    private static IEnumerable<TransferChange> Changes(Pokemon from, Pokemon arrives, Func<TransferField, string?, string?, TransferChangeReason> why)
    {
        var changes = new List<TransferChange>();

        void Compare(TransferField field, string? before, string? after)
        {
            if (before != after) changes.Add(new TransferChange(field, before, after, why(field, before, after)));
        }

        Compare(TransferField.Species, from.Species.Name, arrives.Species.Name);
        Compare(TransferField.Form, from.Form.Form.Name, arrives.Form.Form.Name);
        Compare(TransferField.HeldItem, HeldItem(from), HeldItem(arrives));

        var (movesBefore, movesAfter) = (Moves(from), Moves(arrives));
        changes.AddRange(movesBefore.Except(movesAfter).Select(move => new TransferChange(TransferField.Moves, move, null, why(TransferField.Moves, move, null))));
        changes.AddRange(movesAfter.Except(movesBefore).Select(move => new TransferChange(TransferField.Moves, null, move, why(TransferField.Moves, null, move))));

        Compare(TransferField.MetLocation, MetLocation(from), MetLocation(arrives));
        Compare(TransferField.MetLevel, MetLevel(from), MetLevel(arrives));
        Compare(TransferField.Ball, Ball(from), Ball(arrives));
        Compare(TransferField.Friendship, from.Pkm is PK1 ? null : from.Friendship.ToString(), arrives.Pkm is PK1 ? null : arrives.Friendship.ToString());
        Compare(TransferField.Nickname, from.Nickname, arrives.Nickname);
        Compare(TransferField.Ability, Ability(from), Ability(arrives));
        Compare(TransferField.Level, from.Level.ToString(), arrives.Level.ToString());
        Compare(TransferField.Nature, Nature(from), Nature(arrives));
        Compare(TransferField.Gender, from.Gender.ToString(), arrives.Gender.ToString());
        Compare(TransferField.Shiny, from.IsShiny.ToString(), arrives.IsShiny.ToString());
        Compare(TransferField.Language, Language(from), Language(arrives));
        Compare(TransferField.OriginalTrainer, from.Pkm.OriginalTrainerName, arrives.Pkm.OriginalTrainerName);
        Compare(TransferField.TrainerId, from.Pkm.DisplayTID.ToString(), arrives.Pkm.DisplayTID.ToString());
        Compare(TransferField.OriginGame, OriginGame(from), OriginGame(arrives));
        Compare(TransferField.MetDate, from.Pkm.MetDate?.ToString("yyyy-MM-dd"), arrives.Pkm.MetDate?.ToString("yyyy-MM-dd"));
        return changes;
    }

    private static bool IsGameBoy(Pokemon pokemon) => pokemon.Pkm.Format <= 2;

    private static string? Ball(Pokemon pokemon) => IsGameBoy(pokemon) ? null : pokemon.Ball.Name;

    private static string? Ability(Pokemon pokemon) => IsGameBoy(pokemon) ? null : pokemon.Ability.Name;

    private static string? Nature(Pokemon pokemon) => IsGameBoy(pokemon) ? null : pokemon.Pkm.Nature.ToString();

    private static string? Language(Pokemon pokemon) => IsGameBoy(pokemon) ? null : ((LanguageID)pokemon.Pkm.Language).ToString();

    private static string? OriginGame(Pokemon pokemon) => IsGameBoy(pokemon) ? null : GameInfo.GetVersionName(pokemon.Pkm.Version);

    // Gen 1 stores no met data and Gen 2 only for Pokémon caught in Crystal, which leaves the fields at 0.
    private static bool HasNoMetData(Pokemon pokemon) => IsGameBoy(pokemon) && pokemon.MetConditions.Location.Id == 0 && pokemon.MetConditions.Level == 0;

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
    internal static PokemonLegality LegalityIn(Game destination, Pokemon arrival)
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
