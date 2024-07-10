using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins;

/// <summary>
/// Marker interface specifying the types of hooks a plugin can have
/// </summary>
public interface IPluginHook
{
    string Description { get; }
}

public interface IRunOnPokemonChange : IPluginHook
{
    Task OnPokemonChange(Pokemon pokemon);
}

public interface IRunOnPokemonSave : IPluginHook
{
    Task OnPokemonSaved(Pokemon pokemon);
}

public interface IRunOnItemChanged : IPluginHook
{
    Task OnItemChanged(ItemChanged item);
    
    public record ItemChanged(ushort Id, uint Count);
}