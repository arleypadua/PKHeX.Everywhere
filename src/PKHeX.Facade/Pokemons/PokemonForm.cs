using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public class PokemonForm(PKM pokemon)
{
    public bool HasForm => pokemon.PersonalInfo.HasForms;

    public FormDefinition? Form
    {
        get => FormRepository.GetFor(pokemon).FirstOrDefault(f => f.Id == pokemon.Form);
        set
        {
            if (value == null)
            {
                pokemon.Form = 0;
                return;
            }

            pokemon.Form = (byte)value.Id;
        }
    }
}