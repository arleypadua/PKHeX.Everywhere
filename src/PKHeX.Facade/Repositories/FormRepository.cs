using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Repositories;

public static class FormRepository
{
    public static IEnumerable<FormDefinition> GetFor(Species species, EntityContext context)
    {
        try
        {
            return FormConverter.GetFormList((ushort)species, GameInfo.Strings.types, GameInfo.Strings.forms, [], context)
                .Select((form, index) => new FormDefinition((ushort)index, form));
        }
        catch (ArgumentOutOfRangeException)
        {
            return Array.Empty<FormDefinition>();
        }
    }

    public static IEnumerable<FormDefinition> GetFor(PKM pokemon) =>
        GetFor((Species)pokemon.Species, pokemon.Context);
    
    public static IEnumerable<FormDefinition> GetFor(Pokemon pokemon) => GetFor(pokemon.Species, pokemon.Pkm.Context);
}

public record FormDefinition(ushort Id, string Name)
{
    public byte ByteId => (byte)Id;

    public bool IsAlolan => Name.Equals("Alola", StringComparison.InvariantCultureIgnoreCase);
    public bool IsGalarian => Name.Equals("Galar", StringComparison.InvariantCultureIgnoreCase);
    
    public static readonly FormDefinition Default = new(0, string.Empty);
}