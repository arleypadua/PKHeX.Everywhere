using Transfers = PKHeX.Facade.Transfers;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// How a Pokémon gets from one save to the other, as the games move it.
/// </summary>
/// <remarks>
/// <c>link</c> trades between saves of the same format. <c>timeCapsule</c> goes either way between Generation 1 and 2.
/// <c>palPark</c> goes from Generation 3 to 4, and <c>pokeTransfer</c> from Generation 4 to 5.
/// <c>unofficial</c> is a route no game has, where PKHeX copies the fields the destination can hold.
/// </remarks>
public enum TransferRoute
{
    Link,
    TimeCapsule,
    PalPark,
    PokeTransfer,
    Unofficial,
}

/// <summary>
/// The routes of an open transfer. A null route means Pokémon can't move that way, and every Pokémon offered that way is refused with <c>noRoute</c>.
/// Only saves loaded with a ROM hack format and Let's Go saves have no route.
/// </summary>
/// <param name="Send">From the loaded save to the partner.</param>
/// <param name="Receive">From the partner to the loaded save.</param>
public record TransferRoutes(TransferRoute? Send, TransferRoute? Receive);

/// <summary>
/// How many empty box slots each save has for Pokémon to arrive in.
/// </summary>
/// <param name="Mine">Empty box slots in the loaded save.</param>
/// <param name="Partner">Empty box slots in the partner save.</param>
public record TransferRoom(int Mine, int Partner);

/// <summary>
/// The transfer <c>transfer.open</c> opened: the partner save, the routes between the saves and the room in each.
/// </summary>
public record TransferSummary(SaveSummary Partner, TransferRoutes Routes, TransferRoom Room);

/// <summary>
/// The Pokémon to move. Each handle points to a party or box slot: in the loaded save for <c>send</c>, and in the partner for <c>receive</c>.
/// </summary>
/// <param name="Send">Pokémon to move from the loaded save to the partner.</param>
/// <param name="Receive">Pokémon to move from the partner to the loaded save.</param>
/// <param name="Arrivals">Offered Pokémon that arrive as given instead of by the usual conversion.</param>
public record TransferOffer(PokemonHandle[] Send, PokemonHandle[] Receive, TransferArrival[]? Arrivals = null);

/// <summary>
/// An offered Pokémon that arrives as <c>bytes</c>, already in the destination's format, with <c>patch</c> applied as <c>pokemon.update</c> applies it, instead of the usual conversion.
/// </summary>
/// <param name="At">The offered Pokémon, as in <c>send</c> or <c>receive</c>. When both offer it, the arrival is for the one sent.</param>
/// <param name="Bytes">The Pokémon in the destination's format, such as <c>transfer.convert</c> returns.</param>
/// <param name="Patch">The fields to change on the arrival.</param>
public record TransferArrival(PokemonHandle At, byte[] Bytes, PokemonPatch Patch);

/// <summary>
/// Which save of the open transfer: <c>mine</c>, the loaded one, or <c>partner</c>.
/// </summary>
public enum TransferSave
{
    Mine,
    Partner,
}

/// <summary>
/// A Pokémon file, such as <c>pokemon.export</c> writes.
/// </summary>
/// <param name="Bytes">The Pokémon, decrypted or encrypted, at party or box size.</param>
/// <param name="Generation">The generation of the game it comes from, which tells formats of the same size apart.</param>
public record PokemonFile(byte[] Bytes, int Generation);

/// <summary>
/// A Pokémon file converted to a save's format by <c>transfer.convert</c>.
/// </summary>
/// <param name="Bytes">The converted Pokémon, in the bytes <c>pokemon.export</c> writes.</param>
/// <param name="Pokemon">Its details, with <c>legality</c> judged in the save it was converted to.</param>
public record ConvertedPokemon(byte[] Bytes, EditablePokemon Pokemon);

/// <summary>
/// Which way a Pokémon moves: <c>send</c> from the loaded save to the partner, <c>receive</c> from the partner to the loaded save.
/// </summary>
public enum TransferDirection
{
    Send,
    Receive,
}

/// <summary>
/// Why a Pokémon can't be transferred.
/// </summary>
/// <remarks>
/// <c>noRoute</c>: a save loaded with a ROM hack format or a Let's Go save is on either side. <c>slotLocked</c>: its box slot is locked by the game, such as a battle team.
/// <c>eggAcrossGenerations</c>: only link trades take eggs. <c>languageMismatch</c>: a Japanese Generation 1 or 2 Pokémon can't go to an international save, nor the other way.
/// <c>speciesNotInGame</c>: the destination game doesn't have its species or form. <c>lastPartyMember</c>: the sending party must keep a Pokémon that isn't an egg.
/// <c>noRoom</c>: the destination has no empty box slot left for it. <c>bagFull</c>: Platinum can't put back the Griseous Orb it takes before a link trade.
/// <c>conversionFailed</c>: PKHeX can't convert it to the destination's format.
/// </remarks>
public enum TransferRefusal
{
    NoRoute,
    SlotLocked,
    EggAcrossGenerations,
    LanguageMismatch,
    SpeciesNotInGame,
    LastPartyMember,
    NoRoom,
    BagFull,
    ConversionFailed,
}

/// <summary>
/// A field of a Pokémon that a transfer can change.
/// </summary>
/// <remarks>
/// <c>shiny</c> is <c>True</c> or <c>False</c>, <c>trainerId</c> the ID the game shows, and <c>metDate</c> a date such as <c>2009-01-31</c>.
/// </remarks>
public enum TransferField
{
    Species,
    Form,
    HeldItem,
    Moves,
    MetLocation,
    MetLevel,
    Ball,
    Friendship,
    Nickname,
    Ability,
    Level,
    Nature,
    Gender,
    Shiny,
    Language,
    OriginalTrainer,
    TrainerId,
    OriginGame,
    MetDate,
}

/// <summary>
/// Why a transfer changes a field.
/// </summary>
/// <remarks>
/// <c>hmRemoved</c>: Pal Park and Poké Transfer take away HM moves. <c>itemRemapped</c>: the item becomes another one in the destination game.
/// <c>itemRemoved</c>: the destination game can't hold the item. <c>received</c>: a Generation 2, 3 or 4 game sets friendship to 70 on a Pokémon it receives by link trade.
/// <c>tradeEvolution</c>: the destination game evolves the Pokémon on arrival. <c>itemUsed</c>: the evolution uses up the held item.
/// <c>formReverted</c>: Platinum reverts Giratina, Shaymin and Rotom to their base form before a link trade, and takes back the Griseous Orb.
/// <c>notInGame</c>: on the unofficial route, the destination game doesn't have the move, item, ball or ability. A ball becomes a Poké Ball.
/// <c>unofficial</c>: PKHeX changed it converting the Pokémon outside the games' routes, such as the move a Pokémon left with no moves learns.
/// The others name the route that made the change.
/// </remarks>
public enum TransferChangeReason
{
    HmRemoved,
    ItemRemapped,
    ItemRemoved,
    Link,
    TimeCapsule,
    PalPark,
    PokeTransfer,
    Received,
    TradeEvolution,
    ItemUsed,
    FormReverted,
    NotInGame,
    Unofficial,
}

/// <summary>
/// What a transfer changes in a save besides the Pokémon itself.
/// </summary>
/// <remarks>
/// <c>pokedexCaught</c>: the Pokédex registers the species as caught. <c>itemReturned</c>: an item the Pokémon held goes back to the sender's bag.
/// <c>eventVar</c>: the game sets an event variable, as Platinum does to start its Arceus event when it receives a Pokémon from a distribution.
/// </remarks>
public enum TransferSaveChangeKind
{
    PokedexCaught,
    ItemReturned,
    EventVar,
}

/// <summary>
/// A save in a transfer: <c>sender</c> is the one the Pokémon leaves, <c>receiver</c> the one it arrives in.
/// </summary>
public enum TransferSide
{
    Sender,
    Receiver,
}

/// <summary>
/// A field a transfer changes.
/// </summary>
/// <param name="Before">The value as the source game names it. Null when there was none, such as a move the Pokémon learns.</param>
/// <param name="After">The value as the destination game names it. Null when there is none, such as a removed move or item.</param>
public record TransferChange(TransferField Field, string? Before, string? After, TransferChangeReason Reason);

/// <summary>
/// A change a transfer makes to a save.
/// </summary>
/// <param name="Save">The save that changes.</param>
/// <param name="Label">What changes, for display: the species caught, the item returned, or the event and its new value.</param>
public record TransferSaveChange(TransferSaveChangeKind Kind, TransferSide Save, string Label);

/// <summary>
/// A Pokémon in the offer that can be transferred, and what moving it changes.
/// </summary>
/// <param name="Route">How the Pokémon gets to the other save.</param>
/// <param name="From">The Pokémon as it is now. Its handle points to the save it leaves.</param>
/// <param name="Arrives">The Pokémon as it will be in the other save. Its handle is the box slot it will land in.</param>
/// <param name="Changes">The fields the move changes, in the order of <see cref="TransferField"/>. A move can show up once per move removed. For a Pokémon with an arrival, the Pokémon as it is against the arrival, with the route's reasons.</param>
/// <param name="SaveChanges">What the move changes in either save.</param>
/// <param name="Legality">PKHeX's legality check of the Pokémon as it arrives, judged in the destination game.</param>
public record OfferedPokemon(
    TransferDirection Direction,
    TransferRoute Route,
    PokemonSummary From,
    PokemonSummary Arrives,
    TransferChange[] Changes,
    TransferSaveChange[] SaveChanges,
    Legality Legality);

/// <summary>
/// A Pokémon in the offer that can't be transferred.
/// </summary>
/// <param name="At">Where it is, in the save it would leave.</param>
public record RefusedPokemon(TransferDirection Direction, PokemonHandle At, TransferRefusal Reason);

/// <summary>
/// What <c>transfer.commit</c> would do with an offer, returned by <c>transfer.preview</c>.
/// </summary>
/// <param name="Offers">The Pokémon that can be transferred, sent ones first.</param>
/// <param name="Refused">The Pokémon that can't. <c>transfer.commit</c> fails while any are left.</param>
public record TransferPreview(OfferedPokemon[] Offers, RefusedPokemon[] Refused);

/// <summary>
/// A Pokémon a transfer moved.
/// </summary>
/// <param name="At">The box slot it landed in: in the partner for <c>send</c>, in the loaded save for <c>receive</c>.</param>
public record TransferredPokemon(TransferDirection Direction, PokemonId Id, PokemonHandle At);

/// <summary>
/// Both saves after <c>transfer.commit</c>, ready to write back.
/// </summary>
/// <param name="Save">The loaded save.</param>
/// <param name="Partner">The partner save.</param>
/// <param name="Arrived">Where each Pokémon landed, sent ones first.</param>
public record TransferResult(ExportedSave Save, ExportedSave Partner, TransferredPokemon[] Arrived);

public static class TransferMapping
{
    public static TransferRoute? ToDto(this Transfers.TransferRoute? route) => route?.ToDto();

    public static TransferRoute ToDto(this Transfers.TransferRoute route) => route switch
    {
        Transfers.TransferRoute.Link => TransferRoute.Link,
        Transfers.TransferRoute.TimeCapsule => TransferRoute.TimeCapsule,
        Transfers.TransferRoute.PalPark => TransferRoute.PalPark,
        Transfers.TransferRoute.PokeTransfer => TransferRoute.PokeTransfer,
        Transfers.TransferRoute.Unofficial => TransferRoute.Unofficial,
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, null),
    };

    public static TransferDirection ToDto(this Transfers.TransferDirection direction) =>
        direction == Transfers.TransferDirection.Send ? TransferDirection.Send : TransferDirection.Receive;

    public static TransferRefusal ToDto(this Transfers.TransferRefusal refusal) => refusal switch
    {
        Transfers.TransferRefusal.NoRoute => TransferRefusal.NoRoute,
        Transfers.TransferRefusal.SlotLocked => TransferRefusal.SlotLocked,
        Transfers.TransferRefusal.EggAcrossGenerations => TransferRefusal.EggAcrossGenerations,
        Transfers.TransferRefusal.LanguageMismatch => TransferRefusal.LanguageMismatch,
        Transfers.TransferRefusal.SpeciesNotInGame => TransferRefusal.SpeciesNotInGame,
        Transfers.TransferRefusal.LastPartyMember => TransferRefusal.LastPartyMember,
        Transfers.TransferRefusal.NoRoom => TransferRefusal.NoRoom,
        Transfers.TransferRefusal.BagFull => TransferRefusal.BagFull,
        Transfers.TransferRefusal.ConversionFailed => TransferRefusal.ConversionFailed,
        _ => throw new ArgumentOutOfRangeException(nameof(refusal), refusal, null),
    };

    public static TransferChange ToDto(this Transfers.TransferChange change) => new(
        change.Field switch
        {
            Transfers.TransferField.Species => TransferField.Species,
            Transfers.TransferField.Form => TransferField.Form,
            Transfers.TransferField.HeldItem => TransferField.HeldItem,
            Transfers.TransferField.Moves => TransferField.Moves,
            Transfers.TransferField.MetLocation => TransferField.MetLocation,
            Transfers.TransferField.MetLevel => TransferField.MetLevel,
            Transfers.TransferField.Ball => TransferField.Ball,
            Transfers.TransferField.Friendship => TransferField.Friendship,
            Transfers.TransferField.Nickname => TransferField.Nickname,
            Transfers.TransferField.Ability => TransferField.Ability,
            Transfers.TransferField.Level => TransferField.Level,
            Transfers.TransferField.Nature => TransferField.Nature,
            Transfers.TransferField.Gender => TransferField.Gender,
            Transfers.TransferField.Shiny => TransferField.Shiny,
            Transfers.TransferField.Language => TransferField.Language,
            Transfers.TransferField.OriginalTrainer => TransferField.OriginalTrainer,
            Transfers.TransferField.TrainerId => TransferField.TrainerId,
            Transfers.TransferField.OriginGame => TransferField.OriginGame,
            Transfers.TransferField.MetDate => TransferField.MetDate,
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.Field, null),
        },
        change.Before,
        change.After,
        change.Reason switch
        {
            Transfers.TransferChangeReason.HmRemoved => TransferChangeReason.HmRemoved,
            Transfers.TransferChangeReason.ItemRemapped => TransferChangeReason.ItemRemapped,
            Transfers.TransferChangeReason.ItemRemoved => TransferChangeReason.ItemRemoved,
            Transfers.TransferChangeReason.Link => TransferChangeReason.Link,
            Transfers.TransferChangeReason.TimeCapsule => TransferChangeReason.TimeCapsule,
            Transfers.TransferChangeReason.PalPark => TransferChangeReason.PalPark,
            Transfers.TransferChangeReason.PokeTransfer => TransferChangeReason.PokeTransfer,
            Transfers.TransferChangeReason.Received => TransferChangeReason.Received,
            Transfers.TransferChangeReason.TradeEvolution => TransferChangeReason.TradeEvolution,
            Transfers.TransferChangeReason.ItemUsed => TransferChangeReason.ItemUsed,
            Transfers.TransferChangeReason.FormReverted => TransferChangeReason.FormReverted,
            Transfers.TransferChangeReason.NotInGame => TransferChangeReason.NotInGame,
            Transfers.TransferChangeReason.Unofficial => TransferChangeReason.Unofficial,
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.Reason, null),
        });

    public static TransferSaveChange ToDto(this Transfers.TransferSaveChange change) => new(
        change.Kind switch
        {
            Transfers.TransferSaveChangeKind.PokedexCaught => TransferSaveChangeKind.PokedexCaught,
            Transfers.TransferSaveChangeKind.ItemReturned => TransferSaveChangeKind.ItemReturned,
            Transfers.TransferSaveChangeKind.EventVar => TransferSaveChangeKind.EventVar,
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.Kind, null),
        },
        change.Save == Transfers.TransferSide.Sender ? TransferSide.Sender : TransferSide.Receiver,
        change.Label);

    public static TransferRoom ToDto(this Transfers.TransferRoom room) => new(room.Mine, room.Partner);

    public static Transfers.TransferSave ToFacade(this TransferSave save) => save == TransferSave.Mine ? Transfers.TransferSave.Mine : Transfers.TransferSave.Partner;
}
