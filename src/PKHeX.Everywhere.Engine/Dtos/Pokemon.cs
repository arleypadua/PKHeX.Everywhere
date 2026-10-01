using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine.Dtos;

[Branded]
public readonly record struct PokemonId(string Value);

public enum SlotSource
{
    Party,
    Box,
}

public record PokemonHandle(SlotSource Source, int Slot, int? Box = null)
{
    public static PokemonHandle Party(int slot) => new(SlotSource.Party, slot);
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
}
