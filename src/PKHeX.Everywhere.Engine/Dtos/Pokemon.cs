using System.Globalization;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// Identifies a Pokémon by its species and PID. Treat it as opaque: it changes when the species or PID changes, and in Gen 1 and 2 when any of its data changes.
/// </summary>
[Branded]
public readonly record struct PokemonId(string Value);

/// <summary>
/// Where a Pokémon is: the party, a box, or the draft opened with <c>pokemon.edit()</c> or <c>pokemon.clone()</c>.
/// </summary>
public enum SlotSource
{
    Party,
    Box,
    Draft,
}

/// <summary>
/// Points to a Pokémon in the party, a box or the draft.
/// </summary>
/// <param name="Source">Which storage the Pokémon is in.</param>
/// <param name="Slot">Zero-based slot in the party or in the box. Ignored for the draft.</param>
/// <param name="Box">Zero-based box number. Required when <c>source</c> is <c>box</c>.</param>
public record PokemonHandle(SlotSource Source, int Slot, int? Box = null) : IHandle
{
    public static PokemonHandle Party(int slot) => new(SlotSource.Party, slot);
    public static PokemonHandle InBox(int box, int slot) => new(SlotSource.Box, slot, box);
    public static PokemonHandle Draft() => new(SlotSource.Draft, 0);

    public string Topic() => Source switch
    {
        SlotSource.Party => Topics.Party,
        SlotSource.Draft => Topics.Draft,
        _ => Box is { } box ? $"{Topics.Box}/{box}" : Topics.Box,
    };
}

/// <summary>
/// A Pokémon's form.
/// </summary>
/// <param name="Id">Form index within the species' forms. 0 is the default form.</param>
public record PokemonForm(int Id, string Name);

/// <summary>
/// The basics of a Pokémon, as listed in the party and the boxes.
/// </summary>
/// <param name="At">Where the Pokémon is.</param>
/// <param name="SpeciesId">PKHeX species id, which is the National Pokédex number, or an id of its own for a species the save defines, such as a ROM hack's Shadow Warrior. A sprite lookup by id can miss for those. Null when <c>isUnknown</c> is true.</param>
/// <param name="Species">Species name, or a name like <c>Unknown (#412)</c> when <c>isUnknown</c> is true.</param>
/// <param name="IsUnknown">Neither PKHeX nor the save knows the species, such as a ROM hack's egg slot. Show a placeholder instead of a sprite.</param>
/// <param name="Editable">The Pokémon can be opened with <c>pokemon.edit()</c> and copied with <c>pokemon.clone()</c>. When false, show it read-only with <c>pokemon.details()</c>: editing it fails with <c>unknown-species</c>.</param>
/// <param name="Nickname">The species name when the Pokémon has no nickname.</param>
public record PokemonSummary(
    PokemonId Id,
    PokemonHandle At,
    int? SpeciesId,
    string Species,
    bool IsUnknown,
    bool Editable,
    PokemonForm Form,
    string Nickname,
    int Level,
    bool IsShiny);

/// <summary>
/// A Pokémon just added to a box.
/// </summary>
/// <param name="At">The empty box slot it was placed in.</param>
public record AddedPokemon(PokemonId Id, PokemonHandle At);

/// <summary>
/// A Pokémon exported as a PKHeX entity file.
/// </summary>
/// <param name="Bytes">The decrypted file contents, in the save's format and at party size.</param>
/// <param name="FileName">Suggested file name, with the extension of the format, such as <c>.pk9</c>.</param>
public record ExportedPokemon(byte[] Bytes, string FileName);

/// <summary>
/// The result of PKHeX's legality check on a Pokémon.
/// </summary>
/// <param name="Valid">True when PKHeX finds nothing illegal.</param>
/// <param name="Messages">One message per failed check, as <c>Check: explanation</c>.</param>
public record Legality(bool Valid, string[] Messages);

/// <summary>
/// A Pokémon's gender.
/// </summary>
public enum PokemonGender
{
    Male,
    Female,
    Genderless,
}

/// <summary>
/// Who currently holds a Pokémon: its original trainer, or the trainer it was traded to.
/// </summary>
public enum PokemonHandler
{
    OriginalTrainer,
    HandlingTrainer,
}

/// <summary>
/// The editable fields of a Pokémon. Send changes back with <c>pokemon.update()</c> as a <see cref="PokemonPatch"/>.
/// </summary>
/// <param name="Species">PKHeX species id, which is the National Pokédex number, or an id of its own for a species the save defines, such as a ROM hack's Shadow Warrior. A sprite lookup by id can miss for those. Null when <c>isUnknown</c> is true.</param>
/// <param name="IsUnknown">Neither PKHeX nor the save knows the species, such as a ROM hack's egg slot.</param>
/// <param name="Editable">When false, show the fields read-only: <c>pokemon.update()</c> fails with <c>unknown-species</c>.</param>
/// <param name="Form">Form index within the species' forms. 0 is the default form.</param>
/// <param name="Nature">PKHeX nature id. See <c>game.natures()</c>.</param>
/// <param name="Ability">PKHeX ability id, or 0 in games without abilities.</param>
/// <param name="HeldItem">PKHeX item id. 0 means no item. See <c>heldItems</c> in <c>pokemon.options()</c>.</param>
/// <param name="HeldItemIsUnknown">True when the Pokémon holds an item PKHeX has no id for, such as a ROM hack's own item. <c>heldItem</c> is then an id of its own, named in <c>heldItems</c> in <c>pokemon.options()</c>, and the item stays until <c>heldItem</c> is set to another item or 0.</param>
/// <param name="Ball">PKHeX ball id. See <c>game.balls()</c>.</param>
/// <param name="Friendship">Friendship with the current handler, 0 to 255.</param>
/// <param name="Language">PKHeX language id. See <c>game.languages()</c>.</param>
/// <param name="IsAlpha">Null in games without alpha Pokémon. Only Legends: Arceus and Legends: Z-A have them.</param>
/// <param name="Nickname">The species name when the Pokémon has no nickname.</param>
/// <param name="Pid">Personality value (PID).</param>
/// <param name="Types">PKHeX type ids (0 is Normal). One entry for a single-type Pokémon.</param>
/// <param name="IsInfected">Has Pokérus.</param>
/// <param name="IsCured">Had Pokérus and is cured.</param>
/// <param name="TrainerId">Original trainer's ID as the game shows it: 6 digits from Gen 7 on, 5 digits before.</param>
/// <param name="SecretId">Original trainer's secret ID, in the same format as <c>trainerId</c>.</param>
/// <param name="HandlingTrainerName">Name of the trainer it was last traded to.</param>
/// <param name="Version">PKHeX game version id of the game it comes from. See <c>game.originGames()</c>.</param>
/// <param name="MetLocation">Location id from the origin game's list. See <c>metLocations</c> in <c>pokemon.options()</c>.</param>
/// <param name="MetDate">Date it was met, as <c>yyyy-MM-dd</c>. Null when the game stores none.</param>
/// <param name="FatefulEncounter">The fateful encounter flag that event Pokémon carry.</param>
/// <param name="Ivs">Individual values: 0 to 31 each, or 0 to 15 in Gen 1 and 2.</param>
/// <param name="Evs">Effort values: up to 252 each from Gen 6 on, 255 in Gen 3 to 5, 65535 in Gen 1 and 2.</param>
/// <param name="Avs">Awakening values, 0 to 200 each. Null outside the Let's Go games.</param>
/// <param name="Stats">The stats the game calculates for the Pokémon.</param>
/// <param name="HiddenPower">Null in games without Hidden Power: Gen 1, Let's Go and Gen 8 on.</param>
/// <param name="CombatPower">Stored Combat Power. Null outside the Let's Go games.</param>
/// <param name="CalculatedCombatPower">The Combat Power the game would calculate from the current stats. Null outside the Let's Go games.</param>
/// <param name="Moves">Always four slots. An empty slot has id 0.</param>
/// <param name="Legality">PKHeX's legality check of the Pokémon as it is now, or null when the save doesn't support <c>legality</c>.</param>
public record EditablePokemon(
    int? Species,
    bool IsUnknown,
    bool Editable,
    int Form,
    PokemonGender Gender,
    int Nature,
    int Ability,
    int HeldItem,
    bool HeldItemIsUnknown,
    int Ball,
    int Friendship,
    int Language,
    bool IsShiny,
    bool? IsAlpha,
    bool IsEgg,
    string Nickname,
    int Level,
    uint Pid,
    int[] Types,
    bool IsInfected,
    bool IsCured,
    uint TrainerId,
    uint SecretId,
    string OriginalTrainerName,
    TrainerGender OriginalTrainerGender,
    string HandlingTrainerName,
    TrainerGender HandlingTrainerGender,
    PokemonHandler CurrentHandler,
    int Version,
    int MetLocation,
    int MetLevel,
    string? MetDate,
    bool FatefulEncounter,
    StatValues Ivs,
    StatValues Evs,
    StatValues? Avs,
    StatValues Stats,
    HiddenPower? HiddenPower,
    int? CombatPower,
    int? CalculatedCombatPower,
    MoveSlot[] Moves,
    Legality? Legality);

/// <summary>
/// A value for each of a Pokémon's six stats.
/// </summary>
public record StatValues(int Health, int Attack, int Defense, int SpecialAttack, int SpecialDefense, int Speed);

/// <summary>
/// The type and power of a Pokémon's Hidden Power.
/// </summary>
/// <param name="Type">Type name.</param>
/// <param name="Power">Base power. Null from Gen 6 on, where it is always 60.</param>
public record HiddenPower(string Type, int? Power);

/// <summary>
/// One of a Pokémon's four move slots.
/// </summary>
/// <param name="Id">PKHeX move id, or the save's own id for an unknown move. 0 means the slot is empty.</param>
/// <param name="Pp">Current PP.</param>
/// <param name="MaxPp">Maximum PP, including PP Ups.</param>
/// <param name="IsUnknown">The slot holds a move PKHeX has no id for, such as a ROM hack's own move, named like <c>Unknown move #90</c>. Its <c>id</c> can only stay in this slot, and the slot can be set to another move or to 0.</param>
public record MoveSlot(int Id, string Name, int Pp, int MaxPp, bool IsUnknown);

/// <summary>
/// Changes to apply with <c>pokemon.update()</c>. Missing or null fields stay as they are. If any field is invalid, the update fails with <c>invalid-patch</c> and nothing changes.
/// Fields are checked in order (species before form and ability, origin game before met location), so one patch can change related fields together.
/// </summary>
/// <param name="Species">PKHeX species id. Must exist in the save's game.</param>
/// <param name="Form">Form index. See <c>forms</c> in <c>pokemon.options()</c>.</param>
/// <param name="Nature">PKHeX nature id from <c>game.natures()</c>. Rejected when <c>locked</c> in <c>pokemon.options()</c> has <c>nature</c>.</param>
/// <param name="Ability">PKHeX ability id. See <c>abilities</c> in <c>pokemon.options()</c>.</param>
/// <param name="HeldItem">PKHeX item id from <c>heldItems</c> in <c>pokemon.options()</c>.</param>
/// <param name="Ball">PKHeX ball id from <c>game.balls()</c>.</param>
/// <param name="Friendship">0 to 255.</param>
/// <param name="Language">PKHeX language id from <c>game.languages()</c>.</param>
/// <param name="IsAlpha">Only in games with alpha Pokémon.</param>
/// <param name="Nickname">Trimmed. An empty string resets it to the species name.</param>
/// <param name="Level">1 to 100.</param>
/// <param name="TrainerId">In the same format as <c>trainerId</c> on <see cref="EditablePokemon"/>. Can be set without <c>secretId</c>.</param>
/// <param name="SecretId">In the same format as <c>secretId</c> on <see cref="EditablePokemon"/>. Can be set without <c>trainerId</c>.</param>
/// <param name="Version">PKHeX game version id from <c>game.originGames()</c>. If the new game uses a different location list, the met location resets to a suggested one.</param>
/// <param name="MetLocation">Location id from <c>metLocations</c> in <c>pokemon.options()</c>, checked against the origin game after any <c>version</c> change.</param>
/// <param name="MetLevel">0 to 100.</param>
/// <param name="MetDate">A <c>yyyy-MM-dd</c> date between 2000 and 2099.</param>
/// <param name="Avs">Only in the Let's Go games. 0 to 200 each.</param>
/// <param name="CombatPower">Only in the Let's Go games. 0 to 65535. Changing <c>ivs</c>, <c>evs</c> or <c>avs</c> recalculates Combat Power unless this is also set.</param>
/// <param name="Moves">PKHeX move ids for all four slots, 0 for an empty slot. All zeros changes nothing.</param>
public record PokemonPatch(
    int? Species = null,
    int? Form = null,
    PokemonGender? Gender = null,
    int? Nature = null,
    int? Ability = null,
    int? HeldItem = null,
    int? Ball = null,
    int? Friendship = null,
    int? Language = null,
    bool? IsShiny = null,
    bool? IsAlpha = null,
    bool? IsEgg = null,
    string? Nickname = null,
    int? Level = null,
    uint? TrainerId = null,
    uint? SecretId = null,
    string? OriginalTrainerName = null,
    TrainerGender? OriginalTrainerGender = null,
    string? HandlingTrainerName = null,
    TrainerGender? HandlingTrainerGender = null,
    PokemonHandler? CurrentHandler = null,
    int? Version = null,
    int? MetLocation = null,
    int? MetLevel = null,
    string? MetDate = null,
    bool? FatefulEncounter = null,
    StatPatch? Ivs = null,
    StatPatch? Evs = null,
    StatPatch? Avs = null,
    int? CombatPower = null,
    int[]? Moves = null);

/// <summary>
/// Per-stat changes to IVs, EVs or AVs. Missing or null stats stay as they are. Each value must be in the game's range, listed on <see cref="EditablePokemon"/>.
/// </summary>
public record StatPatch(
    int? Health = null,
    int? Attack = null,
    int? Defense = null,
    int? SpecialAttack = null,
    int? SpecialDefense = null,
    int? Speed = null);

/// <summary>
/// An id and its display name. The id space depends on the list, such as nature ids from <c>game.natures()</c>.
/// </summary>
public record Choice(int Id, string Name);

/// <summary>
/// An item a Pokémon can hold.
/// </summary>
/// <param name="Id">PKHeX item id. 0 means no item.</param>
/// <param name="IsUnknown">True for an item PKHeX has no id for, such as a ROM hack's own item. Only the Pokémon already holding it is offered it.</param>
public record ItemChoice(int Id, string Name, bool IsUnknown);

/// <summary>
/// The values a specific Pokémon can take for the fields whose choices depend on it.
/// </summary>
/// <param name="Species">Its evolution line: pre-evolutions, evolutions and itself, limited to species in the save's game.</param>
/// <param name="Abilities">Its species' abilities and its current one first, then every other ability by name from Gen 4 on. Empty in games without abilities.</param>
/// <param name="Forms">Empty when the species has no forms.</param>
/// <param name="MetLocations">Locations of its origin game. Empty in Gen 1.</param>
/// <param name="Moves">Moves it learns by level up at or below its current level, plus the legal moves it already knows, sorted by name.</param>
/// <param name="HeldItems">The items the save can store as held items, as <c>game.heldItems()</c> lists them, plus the unknown item it already holds.</param>
/// <param name="Locked">Fields the save can't change, such as <c>nature</c> in Gen 3 and 4, where it comes from the PID. Show them disabled. An update that changes one fails with <c>invalid-patch</c>.</param>
public record PokemonOptions(Choice[] Species, Choice[] Abilities, Choice[] Forms, Choice[] MetLocations, Choice[] Moves, ItemChoice[] HeldItems, PokemonField[] Locked);

/// <summary>
/// A Pokémon field a save can lock, named as in <see cref="PokemonPatch"/>.
/// </summary>
public enum PokemonField
{
    Gender,
    Nature,
    Ability,
    MetLocation,
}

/// <summary>
/// A short description of a Pokémon, sent with events such as <c>pokemonAdded</c> and <c>pokemonSaved</c>.
/// </summary>
/// <param name="SpeciesId">PKHeX species id, which is the National Pokédex number, or an id of its own for a species the save defines, such as a ROM hack's Shadow Warrior. A sprite lookup by id can miss for those.</param>
/// <param name="Gender">Gender name, such as <c>Male</c>, <c>Female</c> or <c>Genderless</c>.</param>
/// <param name="Ball">Name of the ball it was caught in.</param>
public record PokemonOverview(int SpeciesId, string Species, string Gender, string Ball, int Level);

public static class PokemonMapping
{
    public static PokemonSummary ToSummary(this Pokemon pokemon, PokemonHandle at)
    {
        var form = pokemon.Form.Form;
        return new PokemonSummary(
            new PokemonId(pokemon.UniqueId.Value),
            at,
            pokemon.IsUnknown ? null : pokemon.Species.Id,
            pokemon.Species.Name,
            pokemon.IsUnknown,
            pokemon.IsEditable,
            new PokemonForm(form.Id, form.Name),
            pokemon.Nickname,
            pokemon.Level,
            pokemon.IsShiny);
    }

    public static PokemonOverview ToOverview(this Pokemon pokemon) =>
        new(pokemon.Species.Id, pokemon.Species.Name, pokemon.Gender.Name, pokemon.Ball.Name, pokemon.Level);

    public static EditablePokemon ToEditable(this PokemonDetails details) => new(
        details.IsUnknown ? null : details.Species,
        details.IsUnknown,
        details.IsEditable,
        details.Form,
        details.Gender.ToDto(),
        details.Nature,
        details.Ability,
        details.HeldItem,
        details.HeldItemIsUnknown,
        details.Ball,
        details.Friendship,
        details.Language,
        details.IsShiny,
        details.IsAlpha,
        details.IsEgg,
        details.Nickname,
        details.Level,
        details.Pid,
        details.Types.ToArray(),
        details.IsInfected,
        details.IsCured,
        details.TrainerId,
        details.SecretId,
        details.OriginalTrainerName,
        details.OriginalTrainerGender.ToTrainerGender(),
        details.HandlingTrainerName,
        details.HandlingTrainerGender.ToTrainerGender(),
        details.CurrentHandler.ToDto(),
        details.Version,
        details.MetLocation,
        details.MetLevel,
        details.MetDate?.ToString(DateFormat, CultureInfo.InvariantCulture),
        details.FatefulEncounter,
        details.Ivs.ToDto(),
        details.Evs.ToDto(),
        details.Avs?.ToDto(),
        details.Stats.ToDto(),
        details.HiddenPower is { } hiddenPower ? new HiddenPower(hiddenPower.Type, hiddenPower.Power) : null,
        details.CombatPower,
        details.CalculatedCombatPower,
        details.Moves.Select(move => new MoveSlot(move.Id, move.Name, move.Pp, move.MaxPp, move.IsUnknown)).ToArray(),
        details.Legality is { } legality ? new Legality(legality.Valid, legality.Messages.ToArray()) : null);

    public static Facade.Pokemons.PokemonPatch ToFacade(this PokemonPatch patch) => new(
        patch.Species,
        patch.Form,
        patch.Gender?.ToGender(),
        patch.Nature,
        patch.Ability,
        patch.HeldItem,
        patch.Ball,
        patch.Friendship,
        patch.Language,
        patch.IsShiny,
        patch.IsAlpha,
        patch.IsEgg,
        patch.Nickname,
        patch.Level,
        patch.TrainerId,
        patch.SecretId,
        patch.OriginalTrainerName,
        patch.OriginalTrainerGender?.ToGender(),
        patch.HandlingTrainerName,
        patch.HandlingTrainerGender?.ToGender(),
        patch.CurrentHandler?.ToHandler(),
        patch.Version,
        patch.MetLocation,
        patch.MetLevel,
        patch.MetDate is { } metDate ? ParseMetDate(metDate) : null,
        patch.FatefulEncounter,
        patch.Ivs?.ToFacade(),
        patch.Evs?.ToFacade(),
        patch.Avs?.ToFacade(),
        patch.CombatPower,
        patch.Moves);

    private static StatValues ToDto(this Facade.Pokemons.StatValues stats) =>
        new(stats.Health, stats.Attack, stats.Defense, stats.SpecialAttack, stats.SpecialDefense, stats.Speed);

    private static Facade.Pokemons.StatPatch ToFacade(this StatPatch patch) =>
        new(patch.Health, patch.Attack, patch.Defense, patch.SpecialAttack, patch.SpecialDefense, patch.Speed);

    private const string DateFormat = "yyyy-MM-dd";

    private static DateOnly ParseMetDate(string value) =>
        DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new InvalidPatchException(nameof(PokemonPatch.MetDate), $"Met date must be a yyyy-MM-dd date, got \"{value}\".");

    public static PokemonHandler ToDto(this Owner.Handler handler) =>
        handler == Owner.Handler.SomeoneElse ? PokemonHandler.HandlingTrainer : PokemonHandler.OriginalTrainer;

    public static Owner.Handler ToHandler(this PokemonHandler handler) =>
        handler == PokemonHandler.HandlingTrainer ? Owner.Handler.SomeoneElse : Owner.Handler.OriginalTrainer;

    public static PokemonGender ToDto(this Gender gender) =>
        gender == Gender.Male ? PokemonGender.Male : gender == Gender.Female ? PokemonGender.Female : PokemonGender.Genderless;

    public static Gender ToGender(this PokemonGender gender) => gender switch
    {
        PokemonGender.Male => Gender.Male,
        PokemonGender.Female => Gender.Female,
        _ => Gender.Genderless,
    };

    public static PokemonOptions ToDto(this Facade.Pokemons.PokemonOptions options) => new(
        options.Species.ToChoices(),
        options.Abilities.ToChoices(),
        options.Forms.ToChoices(),
        options.MetLocations.ToChoices(),
        options.Moves.ToChoices(),
        options.HeldItems.Select(item => new ItemChoice(item.Id, item.Name, item.IsUnknown)).ToArray(),
        options.Locked.Order().Select(ToDto).ToArray());

    private static PokemonField ToDto(Facade.Pokemons.PokemonField field) => field switch
    {
        Facade.Pokemons.PokemonField.Gender => PokemonField.Gender,
        Facade.Pokemons.PokemonField.Nature => PokemonField.Nature,
        Facade.Pokemons.PokemonField.Ability => PokemonField.Ability,
        Facade.Pokemons.PokemonField.MetLocation => PokemonField.MetLocation,
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
    };

    public static Choice[] ToChoices(this IEnumerable<Facade.Pokemons.Choice> choices) =>
        choices.Select(choice => new Choice(choice.Id, choice.Name)).ToArray();
}
