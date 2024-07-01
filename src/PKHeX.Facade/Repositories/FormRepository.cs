using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Repositories;

public static class FormRepository
{
    public static IEnumerable<FormDefinition> GetFor(PKM pokemon) =>
        FormConverter.GetFormList(pokemon.Species, GameInfo.Strings.types, GameInfo.Strings.forms, [], pokemon.Context)
            .Select((form, index) => new FormDefinition((ushort)index, form));
    
    public static IEnumerable<FormDefinition> GetFor(Pokemon pokemon) => GetFor(pokemon.Pkm);
}

public record FormDefinition(ushort Id, string Name)
{
    public bool IsAlolan => Name.Equals("Alola", StringComparison.InvariantCultureIgnoreCase);
    public bool IsGalarian => Name.Equals("Galar", StringComparison.InvariantCultureIgnoreCase);
}