using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Trades;

public enum TradeRoute
{
    Link,
    TimeCapsule,
    PalPark,
    PokeTransfer,
}

public enum TradeDirection
{
    Send,
    Receive,
}

public enum TradeRefusal
{
    NoRoute,
    SlotLocked,
    EggAcrossGenerations,
    LanguageMismatch,
    SpeciesNotInGame,
    LastPartyMember,
    NoRoom,
}

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

public enum TradeChangeReason
{
    HmRemoved,
    ItemRemapped,
    ItemRemoved,
    Link,
    TimeCapsule,
    PalPark,
    PokeTransfer,
}

public enum TradeSaveChangeKind
{
    PokedexCaught,
}

/// <param name="Index">The party slot, or the box slot counted across every box.</param>
public record TradeSlot(PokemonSource Source, int Index);

public record TradeOffer(IReadOnlyList<TradeSlot> Send, IReadOnlyList<TradeSlot> Receive);

public record TradeRoom(int Mine, int Partner);

public record TradeChange(TradeField Field, string? Before, string? After, TradeChangeReason Reason);

public record TradeSaveChange(TradeSaveChangeKind Kind, SpeciesDefinition Species);

/// <param name="ArrivesAt">The box slot, counted across every box, the Pokémon lands in.</param>
public record TradedPokemon(
    TradeDirection Direction,
    TradeSlot From,
    Pokemon Pokemon,
    int ArrivesAt,
    Pokemon Arrives,
    IReadOnlyList<TradeChange> Changes,
    IReadOnlyList<TradeSaveChange> SaveChanges,
    PokemonLegality Legality);

public record RefusedPokemon(TradeDirection Direction, TradeSlot From, TradeRefusal Reason);

public record TradePreview(IReadOnlyList<TradedPokemon> Offers, IReadOnlyList<RefusedPokemon> Refused);

/// <param name="BoxIndex">The box slot, counted across every box, the Pokémon landed in.</param>
public record TradeArrival(TradeDirection Direction, int BoxIndex, Pokemon Pokemon);

public class TradeRefusedException(IReadOnlyList<RefusedPokemon> refused)
    : Exception($"{refused.Count} Pokémon in the offer can't be traded: {string.Join(", ", refused.Select(r => r.Reason).Distinct())}.")
{
    public IReadOnlyList<RefusedPokemon> Refused { get; } = refused;
}
