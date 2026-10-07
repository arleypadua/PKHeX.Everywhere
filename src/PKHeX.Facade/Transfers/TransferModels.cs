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
    Restored,
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

/// <param name="KeptCopies">Copies kept by earlier transfers to older games. Each restores what an older game dropped from the offered Pokémon it applies to.</param>
public record TransferOffer(IReadOnlyList<TransferSlot> Send, IReadOnlyList<TransferSlot> Receive, IReadOnlyList<KeptCopy>? KeptCopies = null);

/// <summary>
/// A Pokémon as it was before a transfer to an older game, to restore what that game dropped.
/// </summary>
/// <param name="Generation">The generation of the game it left.</param>
/// <param name="Bytes">The Pokémon in that game's format, as <see cref="Pokemon.ToFile"/> writes it.</param>
public record KeptCopy(string IdentityKey, int Generation, byte[] Bytes);

public record TransferRoom(int Mine, int Partner);

public record TransferChange(TransferField Field, string? Before, string? After, TransferChangeReason Reason);

/// <param name="Label">What changes, for display: the species caught, the item returned or the event and its new value.</param>
public record TransferSaveChange(TransferSaveChangeKind Kind, TransferSide Save, string Label);

/// <param name="ArrivesAt">The box slot, counted across every box, the Pokémon lands in.</param>
/// <param name="KeptCopy">The Pokémon as it leaves, kept when it moves to an older game.</param>
public record TransferredPokemon(
    TransferDirection Direction,
    TransferRoute Route,
    TransferSlot From,
    Pokemon Pokemon,
    int ArrivesAt,
    Pokemon Arrives,
    IReadOnlyList<TransferChange> Changes,
    IReadOnlyList<TransferSaveChange> SaveChanges,
    PokemonLegality Legality,
    KeptCopy? KeptCopy = null);

public record RefusedPokemon(TransferDirection Direction, TransferSlot From, TransferRefusal Reason);

public record TransferPreview(IReadOnlyList<TransferredPokemon> Offers, IReadOnlyList<RefusedPokemon> Refused);

/// <param name="BoxIndex">The box slot, counted across every box, the Pokémon landed in.</param>
public record TransferArrival(TransferDirection Direction, int BoxIndex, Pokemon Pokemon, KeptCopy? KeptCopy = null);

/// <param name="Unofficial">No game can move the Pokémon into the save, so PKHeX copied the fields the save can hold.</param>
public record PokemonImport(Pokemon Arrives, bool Unofficial, IReadOnlyList<TransferChange> Changes)
{
    /// <summary>
    /// PKHeX's legality check of the Pokémon as it arrives, judged in the save. Null when the save has no legality check, such as a ROM hack's.
    /// </summary>
    public PokemonLegality? Legality() => Arrives.Game.Supports(Capability.Legality) ? Transfer.LegalityIn(Arrives.Game, Arrives) : null;
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
