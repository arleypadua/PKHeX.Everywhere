using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public class PokemonForm(PKM pokemon)
{
    public bool HasForm => pokemon.PersonalInfo.HasForms;

    public FormDefinition Form
    {
        get => FormRepository.GetFor(pokemon).FirstOrDefault(f => f.Id == pokemon.Form) ?? FormDefinition.Default;
        set => pokemon.Form = (byte)value.Id;
    }
}