using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Transfers;

public enum TransferRoute
{
    Link,
    TimeCapsule,
    PalPark,
    PokeTransfer,
}

public enum TransferDirection
{
    Send,
    Receive,
}

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
}

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
}

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
}

public enum TransferSaveChangeKind
{
    PokedexCaught,
    ItemReturned,
    EventVar,
}

public enum TransferSide
{
    Sender,
    Receiver,
}

/// <param name="Index">The party slot, or the box slot counted across every box.</param>
public record TransferSlot(PokemonSource Source, int Index);

public record TransferOffer(IReadOnlyList<TransferSlot> Send, IReadOnlyList<TransferSlot> Receive);

public record TransferRoom(int Mine, int Partner);

public record TransferChange(TransferField Field, string? Before, string? After, TransferChangeReason Reason);

/// <param name="Label">What changes, for display: the species caught, the item returned or the event and its new value.</param>
public record TransferSaveChange(TransferSaveChangeKind Kind, TransferSide Save, string Label);

/// <param name="ArrivesAt">The box slot, counted across every box, the Pokémon lands in.</param>
public record TransferredPokemon(
    TransferDirection Direction,
    TransferSlot From,
    Pokemon Pokemon,
    int ArrivesAt,
    Pokemon Arrives,
    IReadOnlyList<TransferChange> Changes,
    IReadOnlyList<TransferSaveChange> SaveChanges,
    PokemonLegality Legality);

public record RefusedPokemon(TransferDirection Direction, TransferSlot From, TransferRefusal Reason);

public record TransferPreview(IReadOnlyList<TransferredPokemon> Offers, IReadOnlyList<RefusedPokemon> Refused);

/// <param name="BoxIndex">The box slot, counted across every box, the Pokémon landed in.</param>
public record TransferArrival(TransferDirection Direction, int BoxIndex, Pokemon Pokemon);

public class TransferRefusedException(IReadOnlyList<RefusedPokemon> refused)
    : Exception($"{refused.Count} Pokémon in the offer can't be transferred: {string.Join(", ", refused.Select(r => r.Reason).Distinct())}.")
{
    public IReadOnlyList<RefusedPokemon> Refused { get; } = refused;
}
