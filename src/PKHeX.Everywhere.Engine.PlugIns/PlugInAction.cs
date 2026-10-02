using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.PlugIns;

/// <summary>
/// Where a plug-in action shows up: <c>quick</c> on the home page with no target, <c>pokemon</c> in the Pokémon editor, <c>pokemonStats</c> on the editor's stats tab.
/// </summary>
public enum ActionPlacement
{
    Quick,
    Pokemon,
    PokemonStats,
}

/// <summary>
/// An action an enabled plug-in hook offers, as listed by <c>plugins.actions</c>. Run it with <c>plugins.run</c>.
/// </summary>
/// <param name="Id">The action's hook id, the full name of its .NET type.</param>
/// <param name="PlugInId">The plug-in's id, its assembly name.</param>
/// <param name="Label">The text for the action's button.</param>
/// <param name="Disabled">Whether the action can't run right now. <c>plugins.run</c> rejects disabled actions.</param>
/// <param name="Reason">Why the action is disabled, or <c>null</c> when it isn't or the plug-in gives no reason.</param>
public sealed record PlugInAction(string Id, string PlugInId, string Label, string Description, bool Disabled, string? Reason);

/// <summary>
/// What a hook asks the host to do after it runs: nothing, show a notification, or open one of the plug-in's pages.
/// </summary>
public enum PlugInOutcomeKind
{
    Void,
    Notify,
    OpenPage,
}

/// <summary>
/// The severity of a notification a plug-in asks the host to show.
/// </summary>
public enum PlugInNotificationType
{
    None,
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// What a hook asks the host to do after it runs.
/// </summary>
/// <param name="Message">The notification's text, set when <c>kind</c> is <c>notify</c>.</param>
/// <param name="Description">Optional detail under the notification's message.</param>
/// <param name="Type">The notification's severity, set when <c>kind</c> is <c>notify</c>.</param>
/// <param name="Path">The path of the page to open, set when <c>kind</c> is <c>openPage</c>. It matches the <c>path</c> of one of the plug-in's declared pages.</param>
public sealed record PlugInOutcome(
    PlugInOutcomeKind Kind,
    string? Message = null,
    string? Description = null,
    PlugInNotificationType? Type = null,
    string? Path = null)
{
    public static PlugInOutcome From(Outcome outcome) => outcome switch
    {
        Outcome.Notification n => new(PlugInOutcomeKind.Notify, n.Message, n.Description, (PlugInNotificationType)n.Type),
        Outcome.PageRequest page => new(PlugInOutcomeKind.OpenPage, Path: page.Path),
        _ => new(PlugInOutcomeKind.Void),
    };
}
