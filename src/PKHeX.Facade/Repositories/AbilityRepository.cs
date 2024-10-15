using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class AbilityRepository
{
    public static readonly AbilityRepository Instance = new();
    
    private readonly Dictionary<int, AbilityDefinition> _abilities;

    private AbilityRepository()
    {
        _abilities = GameInfo.Strings.Ability
            .Select((moveName, id) => (id, moveName))
            .ToDictionary(x => x.id, x => new AbilityDefinition(x.id, x.moveName));
    }
    
    public AbilityDefinition Get(int id) => _abilities.GetValueOrDefault(id)
        ?? AbilityDefinition.None;
    
    public List<AbilityDefinition> All => _abilities.Values.ToList();
}

public record AbilityDefinition(int Id, string Name)
{
    public Ability Ability => (Ability)Id;
    
    public static readonly AbilityDefinition None = new((int)Ability.None, "(None)");
}