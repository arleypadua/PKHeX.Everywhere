using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins;

/// <summary>
/// Marker interface specifying the types of hooks a plugin can have
/// </summary>
public interface IPluginHook
{
    string Description { get; }
}

/// <summary>
/// Runs whenever the user changes any data of a pokemon
/// P.S.: As soon as an input is changed, this will be triggered
/// </summary>
public interface IRunOnPokemonChange : IPluginHook
{
    Task OnPokemonChange(Pokemon pokemon);
}

/// <summary>
/// Runs whenever the user saves a Pokemon
/// </summary>
public interface IRunOnPokemonSave : IPluginHook
{
    Task OnPokemonSaved(Pokemon pokemon);
}

/// <summary>
/// Runs whenever an item is changed
/// </summary>
public interface IRunOnItemChanged : IPluginHook
{
    Task OnItemChanged(ItemChanged item);
    
    public record ItemChanged(ushort Id, uint Count);
}

/// <summary>
/// Adds an action button onto the Pokemon edit UI
/// </summary>
public interface IPokemonEditAction : IPluginHook
{
    string Label { get; }
    Task OnActionRequested(Pokemon pokemon);
}