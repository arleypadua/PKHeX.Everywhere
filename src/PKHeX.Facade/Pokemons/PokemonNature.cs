using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public record PokemonNature(PKM Pokemon)
{
    public Nature Nature => Pokemon.Nature;
    public Nature StatNature => Pokemon.StatAlignment;

    public bool ChangeAll(Nature newNature)
    {
        if (newNature == Pokemon.Nature) return true;
        
        var oldNature = Pokemon.Nature;
        
        Pokemon.Nature = newNature;
        Pokemon.StatAlignment = newNature;

        return Pokemon.Nature != oldNature;
    }

    public override string ToString() => Nature == StatNature
        ? Nature.ToString()
        : $"{Nature} / {StatNature}";
}