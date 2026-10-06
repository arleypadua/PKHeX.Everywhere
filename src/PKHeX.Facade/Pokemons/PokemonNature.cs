using PKHeX.Core;
using PKHeX.Facade.Extensions;

namespace PKHeX.Facade.Pokemons;

public record PokemonNature(PKM Pokemon)
{
    public Nature Nature => Pokemon.Nature;
    public Nature StatNature => Pokemon.StatAlignment;

    public bool ChangeAll(Nature newNature)
    {
        if (newNature == Pokemon.Nature) return true;

        Pokemon.Nature = newNature;
        Pokemon.StatAlignment = newNature;
        // Gen 3 and 4 Pokémon ignore the setter, as their nature comes from the PID.
        if (Pokemon.Nature != newNature && Pokemon.Format is 3 or 4) Pokemon.SetPidNature(newNature);

        return Pokemon.Nature == newNature;
    }

    public override string ToString() => Nature == StatNature
        ? Nature.ToString()
        : $"{Nature} / {StatNature}";
}