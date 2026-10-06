using Trades = PKHeX.Facade.Trades;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// How a Pokémon gets from one save to the other, as the games move it.
/// </summary>
/// <remarks>
/// <c>link</c> trades between saves of the same format. <c>timeCapsule</c> goes either way between Generation 1 and 2.
/// <c>palPark</c> goes from Generation 3 to 4, and <c>pokeTransfer</c> from Generation 4 to 5.
/// </remarks>
public enum TradeRoute
{
    Link,
    TimeCapsule,
    PalPark,
    PokeTransfer,
}

/// <summary>
/// The routes of an open trade. A null route means the games can't move Pokémon that way, and every Pokémon offered that way is refused with <c>noRoute</c>.
/// Saves loaded with a ROM hack format and Let's Go saves have none.
/// </summary>
/// <param name="Send">From the loaded save to the partner.</param>
/// <param name="Receive">From the partner to the loaded save.</param>
public record TradeRoutes(TradeRoute? Send, TradeRoute? Receive);

/// <summary>
/// How many empty box slots each save has for Pokémon to arrive in.
/// </summary>
/// <param name="Mine">Empty box slots in the loaded save.</param>
/// <param name="Partner">Empty box slots in the partner save.</param>
public record TradeRoom(int Mine, int Partner);

/// <summary>
/// The trade <c>trade.open</c> opened: the partner save, the routes between the saves and the room in each.
/// </summary>
public record TradeSummary(SaveSummary Partner, TradeRoutes Routes, TradeRoom Room);

/// <summary>
/// The Pokémon to move. Each handle points to a party or box slot: in the loaded save for <c>send</c>, and in the partner for <c>receive</c>.
/// </summary>
/// <param name="Send">Pokémon to move from the loaded save to the partner.</param>
/// <param name="Receive">Pokémon to move from the partner to the loaded save.</param>
public record TradeOffer(PokemonHandle[] Send, PokemonHandle[] Receive);

/// <summary>
/// Which way a Pokémon moves: <c>send</c> from the loaded save to the partner, <c>receive</c> from the partner to the loaded save.
/// </summary>
public enum TradeDirection
{
    Send,
    Receive,
}

/// <summary>
/// Why a Pokémon can't be traded.
/// </summary>
/// <remarks>
/// <c>noRoute</c>: the games can't move Pokémon that way. <c>slotLocked</c>: its box slot is locked by the game, such as a battle team.
/// <c>eggAcrossGenerations</c>: only link trades take eggs. <c>languageMismatch</c>: a Japanese Generation 1 or 2 Pokémon can't go to an international save, nor the other way.
/// <c>speciesNotInGame</c>: the destination game doesn't have its species or form. <c>lastPartyMember</c>: the sending party must keep a Pokémon that isn't an egg.
/// <c>noRoom</c>: the destination has no empty box slot left for it. <c>bagFull</c>: Platinum can't put back the Griseous Orb it takes before a trade.
/// </remarks>
public enum TradeRefusal
{
    NoRoute,
    SlotLocked,
    EggAcrossGenerations,
    LanguageMismatch,
    SpeciesNotInGame,
    LastPartyMember,
    NoRoom,
    BagFull,
}

/// <summary>
/// A field of a Pokémon that a trade can change.
/// </summary>
public enum TradeField
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
}

/// <summary>
/// Why a trade changes a field.
/// </summary>
/// <remarks>
/// <c>hmRemoved</c>: Pal Park and Poké Transfer take away HM moves. <c>itemRemapped</c>: the item becomes another one in the destination game.
/// <c>itemRemoved</c>: the destination game can't hold the item. <c>received</c>: a Generation 2, 3 or 4 game sets friendship to 70 on a Pokémon it receives by link trade.
/// <c>tradeEvolution</c>: the destination game evolves the Pokémon on arrival. <c>itemUsed</c>: the evolution uses up the held item.
/// <c>formReverted</c>: Platinum reverts Giratina, Shaymin and Rotom to their base form before a trade, and takes back the Griseous Orb.
/// The others name the route that made the change.
/// </remarks>
public enum TradeChangeReason
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
}

/// <summary>
/// What a trade changes in a save besides the Pokémon itself.
/// </summary>
/// <remarks>
/// <c>pokedexCaught</c>: the Pokédex registers the species as caught. <c>itemReturned</c>: an item the Pokémon held goes back to the sender's bag.
/// <c>eventVar</c>: the game sets an event variable, as Platinum does to start its Arceus event when it receives a Pokémon from a distribution.
/// </remarks>
public enum TradeSaveChangeKind
{
    PokedexCaught,
    ItemReturned,
    EventVar,
}

/// <summary>
/// A save in a trade: <c>sender</c> is the one the Pokémon leaves, <c>receiver</c> the one it arrives in.
/// </summary>
public enum TradeSide
{
    Sender,
    Receiver,
}

/// <summary>
/// A field a trade changes.
/// </summary>
/// <param name="Before">The value as the source game names it. Null when there was none, such as a move the Pokémon learns.</param>
/// <param name="After">The value as the destination game names it. Null when there is none, such as a removed move or item.</param>
public record TradeChange(TradeField Field, string? Before, string? After, TradeChangeReason Reason);

/// <summary>
/// A change a trade makes to a save.
/// </summary>
/// <param name="Save">The save that changes.</param>
/// <param name="Label">What changes, for display: the species caught, the item returned, or the event and its new value.</param>
public record TradeSaveChange(TradeSaveChangeKind Kind, TradeSide Save, string Label);

/// <summary>
/// A Pokémon in the offer that can be traded, and what moving it changes.
/// </summary>
/// <param name="From">The Pokémon as it is now. Its handle points to the save it leaves.</param>
/// <param name="Arrives">The Pokémon as it will be in the other save. Its handle is the box slot it will land in.</param>
/// <param name="Changes">The fields the move changes, in the order of <see cref="TradeField"/>. A move can show up once per move removed.</param>
/// <param name="SaveChanges">What the move changes in either save.</param>
/// <param name="Legality">PKHeX's legality check of the Pokémon as it arrives, judged in the destination game.</param>
public record OfferedPokemon(
    TradeDirection Direction,
    PokemonSummary From,
    PokemonSummary Arrives,
    TradeChange[] Changes,
    TradeSaveChange[] SaveChanges,
    Legality Legality);

/// <summary>
/// A Pokémon in the offer that can't be traded.
/// </summary>
/// <param name="At">Where it is, in the save it would leave.</param>
public record RefusedPokemon(TradeDirection Direction, PokemonHandle At, TradeRefusal Reason);

/// <summary>
/// What <c>trade.commit</c> would do with an offer, returned by <c>trade.preview</c>.
/// </summary>
/// <param name="Offers">The Pokémon that can be traded, sent ones first.</param>
/// <param name="Refused">The Pokémon that can't. <c>trade.commit</c> fails while any are left.</param>
public record TradePreview(OfferedPokemon[] Offers, RefusedPokemon[] Refused);

/// <summary>
/// A Pokémon a trade moved.
/// </summary>
/// <param name="At">The box slot it landed in: in the partner for <c>send</c>, in the loaded save for <c>receive</c>.</param>
public record TradedPokemon(TradeDirection Direction, PokemonId Id, PokemonHandle At);

/// <summary>
/// Both saves after <c>trade.commit</c>, ready to write back.
/// </summary>
/// <param name="Save">The loaded save.</param>
/// <param name="Partner">The partner save.</param>
/// <param name="Arrived">Where each Pokémon landed, sent ones first.</param>
public record TradeResult(ExportedSave Save, ExportedSave Partner, TradedPokemon[] Arrived);

public static class TradeMapping
{
    public static TradeRoute? ToDto(this Trades.TradeRoute? route) => route switch
    {
        null => null,
        Trades.TradeRoute.Link => TradeRoute.Link,
        Trades.TradeRoute.TimeCapsule => TradeRoute.TimeCapsule,
        Trades.TradeRoute.PalPark => TradeRoute.PalPark,
        Trades.TradeRoute.PokeTransfer => TradeRoute.PokeTransfer,
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, null),
    };

    public static TradeDirection ToDto(this Trades.TradeDirection direction) =>
        direction == Trades.TradeDirection.Send ? TradeDirection.Send : TradeDirection.Receive;

    public static TradeRefusal ToDto(this Trades.TradeRefusal refusal) => refusal switch
    {
        Trades.TradeRefusal.NoRoute => TradeRefusal.NoRoute,
        Trades.TradeRefusal.SlotLocked => TradeRefusal.SlotLocked,
        Trades.TradeRefusal.EggAcrossGenerations => TradeRefusal.EggAcrossGenerations,
        Trades.TradeRefusal.LanguageMismatch => TradeRefusal.LanguageMismatch,
        Trades.TradeRefusal.SpeciesNotInGame => TradeRefusal.SpeciesNotInGame,
        Trades.TradeRefusal.LastPartyMember => TradeRefusal.LastPartyMember,
        Trades.TradeRefusal.NoRoom => TradeRefusal.NoRoom,
        Trades.TradeRefusal.BagFull => TradeRefusal.BagFull,
        _ => throw new ArgumentOutOfRangeException(nameof(refusal), refusal, null),
    };

    public static TradeChange ToDto(this Trades.TradeChange change) => new(
        change.Field switch
        {
            Trades.TradeField.Species => TradeField.Species,
            Trades.TradeField.Form => TradeField.Form,
            Trades.TradeField.HeldItem => TradeField.HeldItem,
            Trades.TradeField.Moves => TradeField.Moves,
            Trades.TradeField.MetLocation => TradeField.MetLocation,
            Trades.TradeField.MetLevel => TradeField.MetLevel,
            Trades.TradeField.Ball => TradeField.Ball,
            Trades.TradeField.Friendship => TradeField.Friendship,
            Trades.TradeField.Nickname => TradeField.Nickname,
            Trades.TradeField.Ability => TradeField.Ability,
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.Field, null),
        },
        change.Before,
        change.After,
        change.Reason switch
        {
            Trades.TradeChangeReason.HmRemoved => TradeChangeReason.HmRemoved,
            Trades.TradeChangeReason.ItemRemapped => TradeChangeReason.ItemRemapped,
            Trades.TradeChangeReason.ItemRemoved => TradeChangeReason.ItemRemoved,
            Trades.TradeChangeReason.Link => TradeChangeReason.Link,
            Trades.TradeChangeReason.TimeCapsule => TradeChangeReason.TimeCapsule,
            Trades.TradeChangeReason.PalPark => TradeChangeReason.PalPark,
            Trades.TradeChangeReason.PokeTransfer => TradeChangeReason.PokeTransfer,
            Trades.TradeChangeReason.Received => TradeChangeReason.Received,
            Trades.TradeChangeReason.TradeEvolution => TradeChangeReason.TradeEvolution,
            Trades.TradeChangeReason.ItemUsed => TradeChangeReason.ItemUsed,
            Trades.TradeChangeReason.FormReverted => TradeChangeReason.FormReverted,
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.Reason, null),
        });

    public static TradeSaveChange ToDto(this Trades.TradeSaveChange change) => new(
        change.Kind switch
        {
            Trades.TradeSaveChangeKind.PokedexCaught => TradeSaveChangeKind.PokedexCaught,
            Trades.TradeSaveChangeKind.ItemReturned => TradeSaveChangeKind.ItemReturned,
            Trades.TradeSaveChangeKind.EventVar => TradeSaveChangeKind.EventVar,
            _ => throw new ArgumentOutOfRangeException(nameof(change), change.Kind, null),
        },
        change.Save == Trades.TradeSide.Sender ? TradeSide.Sender : TradeSide.Receiver,
        change.Label);

    public static TradeRoom ToDto(this Trades.TradeRoom room) => new(room.Mine, room.Partner);
}
