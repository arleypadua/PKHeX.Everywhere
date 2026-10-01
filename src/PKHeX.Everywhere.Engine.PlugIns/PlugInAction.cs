using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.PlugIns;

public enum ActionPlacement
{
    Quick,
    Pokemon,
    PokemonStats,
}

public sealed record PlugInAction(string Id, string PlugInId, string Label, string Description, bool Disabled, string? Reason);

public enum PlugInOutcomeKind
{
    Void,
    Notify,
    OpenPage,
}

public enum PlugInNotificationType
{
    None,
    Info,
    Success,
    Warning,
    Error,
}

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
