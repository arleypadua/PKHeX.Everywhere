using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public static class NatureRepository
{
    private static readonly Dictionary<int, NatureDefinition> Natures = GameInfo.Strings.natures
        .Select((name, id) => new NatureDefinition(id, name))
        .ToDictionary(nature => nature.Id);

    public static NatureDefinition GetNature(int id) => Natures.GetValueOrDefault(id) ?? NatureDefinition.Unknown(id);
}

public record NatureDefinition(int Id, string Name)
{
    public static NatureDefinition Unknown(int id) => new(id, $"Unknown Nature {id}");
}
