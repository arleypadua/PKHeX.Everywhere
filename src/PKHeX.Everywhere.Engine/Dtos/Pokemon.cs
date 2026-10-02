using System.Globalization;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine.Dtos;

[Branded]
public readonly record struct PokemonId(string Value);

public enum SlotSource
{
    Party,
    Box,
    Draft,
}

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

public record PokemonForm(int Id, string Name);

public record PokemonSummary(
    PokemonId Id,
    PokemonHandle At,
    int SpeciesId,
    string Species,
    PokemonForm Form,
    string Nickname,
    int Level,
    bool IsShiny);

public record AddedPokemon(PokemonId Id, PokemonHandle At);

public record Legality(bool Valid, string[] Messages);

public enum PokemonGender
{
    Male,
    Female,
    Genderless,
}

public enum PokemonHandler
{
    OriginalTrainer,
    HandlingTrainer,
}

public record EditablePokemon(
    int Species,
    int Form,
    PokemonGender Gender,
    int Nature,
    int Ability,
    int HeldItem,
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
    Legality Legality);

public record StatValues(int Health, int Attack, int Defense, int SpecialAttack, int SpecialDefense, int Speed);

public record HiddenPower(string Type, int? Power);

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
    int? CombatPower = null);

public record StatPatch(
    int? Health = null,
    int? Attack = null,
    int? Defense = null,
    int? SpecialAttack = null,
    int? SpecialDefense = null,
    int? Speed = null);

public record Choice(int Id, string Name);

public record PokemonOptions(Choice[] Species, Choice[] Abilities, Choice[] Forms, Choice[] MetLocations);

public record PokemonOverview(int SpeciesId, string Species, string Gender, string Ball, int Level);

public static class PokemonMapping
{
    public static PokemonSummary ToSummary(this Pokemon pokemon, PokemonHandle at)
    {
        var form = pokemon.Form.Form;
        return new PokemonSummary(
            new PokemonId(pokemon.UniqueId.Value),
            at,
            pokemon.Species.Id,
            pokemon.Species.Name,
            new PokemonForm(form.Id, form.Name),
            pokemon.Nickname,
            pokemon.Level,
            pokemon.IsShiny);
    }

    public static PokemonOverview ToOverview(this Pokemon pokemon) =>
        new(pokemon.Species.Id, pokemon.Species.Name, pokemon.Gender.Name, pokemon.Ball.Name, pokemon.Level);

    public static EditablePokemon ToEditable(this PokemonDetails details) => new(
        details.Species,
        details.Form,
        details.Gender.ToDto(),
        details.Nature,
        details.Ability,
        details.HeldItem,
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
        new Legality(details.Legality.Valid, details.Legality.Messages.ToArray()));

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
        patch.CombatPower);

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
        options.MetLocations.ToChoices());

    public static Choice[] ToChoices(this IEnumerable<Facade.Pokemons.Choice> choices) =>
        choices.Select(choice => new Choice(choice.Id, choice.Name)).ToArray();
}
