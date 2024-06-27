using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public record PokemonNature(PKM Pokemon)
{
    public Nature Nature => Pokemon.Nature;
    public Nature StatNature => Pokemon.StatNature;

    public override string ToString() => Nature == StatNature
        ? Nature.ToString()
        : $"{Nature} / {StatNature}";
}