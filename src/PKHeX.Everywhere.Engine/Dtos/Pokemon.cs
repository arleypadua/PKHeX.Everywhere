using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using Choice = PKHeX.Everywhere.Engine.Dtos.Choice;
using PokemonOptions = PKHeX.Everywhere.Engine.Dtos.PokemonOptions;
using PokemonPatch = PKHeX.Everywhere.Engine.Dtos.PokemonPatch;

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
    Legality Legality);

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
    int? Level = null);

public record Choice(int Id, string Name);

public record PokemonOptions(Choice[] Species, Choice[] Abilities, Choice[] Forms);

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

    public static EditablePokemon ToEditable(this PokemonDetails details) => new(
        details.Species,
        details.Form,
        details.Gender == Gender.Male ? PokemonGender.Male : details.Gender == Gender.Female ? PokemonGender.Female : PokemonGender.Genderless,
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
        new Legality(details.Legality.Valid, details.Legality.Messages.ToArray()));

    public static Facade.Pokemons.PokemonPatch ToFacade(this PokemonPatch patch) => new(
        patch.Species,
        patch.Form,
        patch.Gender switch
        {
            PokemonGender.Male => Gender.Male,
            PokemonGender.Female => Gender.Female,
            PokemonGender.Genderless => Gender.Genderless,
            _ => null,
        },
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
        patch.Level);

    public static PokemonOptions ToDto(this Facade.Pokemons.PokemonOptions options) => new(
        options.Species.ToChoices(),
        options.Abilities.ToChoices(),
        options.Forms.ToChoices());

    public static Choice[] ToChoices(this IEnumerable<Facade.Pokemons.Choice> choices) =>
        choices.Select(choice => new Choice(choice.Id, choice.Name)).ToArray();
}
