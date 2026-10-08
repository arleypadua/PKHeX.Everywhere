using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Transfers;

public enum TransferRoute
{
    Link,
    TimeCapsule,
    PalPark,
    PokeTransfer,
    Unofficial,
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
    ConversionFailed,
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

/// <param name="Arrivals">Offered Pokémon that arrive as given instead of by the usual conversion.</param>
public record TransferOffer(IReadOnlyList<TransferSlot> Send, IReadOnlyList<TransferSlot> Receive, IReadOnlyList<ArrivalOverride>? Arrivals = null);

/// <summary>
/// An offered Pokémon that arrives as <paramref name="Bytes"/>, with <paramref name="Patch"/> applied as <see cref="Pokemon.Update"/> applies it, instead of by the usual conversion.
/// </summary>
/// <param name="At">The offered Pokémon, in the save it leaves.</param>
/// <param name="Bytes">The Pokémon in the destination's format, as <see cref="Pokemon.ToFile"/> writes it.</param>
public record ArrivalOverride(TransferDirection Direction, TransferSlot At, byte[] Bytes, PokemonPatch Patch);

public enum TransferSave
{
    Mine,
    Partner,
}

public record TransferRoom(int Mine, int Partner);

public record TransferChange(TransferField Field, string? Before, string? After, TransferChangeReason Reason);

/// <param name="Label">What changes, for display: the species caught, the item returned or the event and its new value.</param>
public record TransferSaveChange(TransferSaveChangeKind Kind, TransferSide Save, string Label);

/// <param name="ArrivesAt">The box slot, counted across every box, the Pokémon lands in.</param>
public record TransferredPokemon(
    TransferDirection Direction,
    TransferRoute Route,
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

/// <param name="Unofficial">No game can move the Pokémon into the save, so PKHeX copied the fields the save can hold.</param>
public record PokemonImport(Pokemon Arrives, bool Unofficial, IReadOnlyList<TransferChange> Changes)
{
    /// <summary>
    /// PKHeX's legality check of the Pokémon as it arrives, judged in the save. Null when the save has no legality check, such as a ROM hack's.
    /// </summary>
    public PokemonLegality? Legality() => Arrives.Game.Supports(Capability.Legality) ? Transfer.LegalityIn(Arrives.Game, Arrives) : null;

    /// <summary>
    /// The Pokémon's details as it arrives, with <see cref="Legality"/> judged in the save.
    /// </summary>
    public PokemonDetails Details() => Arrives.Details(withLegality: false) with { Legality = Legality() };
}

public class PokemonRefusedException(TransferRefusal reason) : Exception($"The Pokémon can't be moved into the save: {reason}.")
{
    public TransferRefusal Reason { get; } = reason;
}

public class TransferRefusedException(IReadOnlyList<RefusedPokemon> refused)
    : Exception($"{refused.Count} Pokémon in the offer can't be transferred: {string.Join(", ", refused.Select(r => r.Reason).Distinct())}.")
{
    public IReadOnlyList<RefusedPokemon> Refused { get; } = refused;
}
