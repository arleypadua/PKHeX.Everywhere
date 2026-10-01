namespace PKHeX.Everywhere.PlugIns;

public abstract class Outcome
{
    // only allow this library to specify valid implementations
    private protected Outcome()
    {
    }

    public static readonly Outcome Void = new VoidOutcome();

    public static Notification Notify(string message, string? description = null,
        Notification.NotificationType type = Notification.NotificationType.None) => new()
    {
        Message = message,
        Description = description,
        Type = type
    };

    /// <summary>
    /// Asks the app to open one of the pages the plug-in declares
    /// </summary>
    public static PageRequest OpenPage(string path) => new() { Path = path };

    public sealed class Notification : Outcome
    {
        public required string Message { get; init; }
        public string? Description { get; init; }
        public NotificationType Type { get; init; } = NotificationType.None;

        public enum NotificationType
        {
            None = 0,
            Info = 1,
            Success = 2,
            Warning = 3,
            Error = 4
        }
    }

    public sealed class PageRequest : Outcome
    {
        public required string Path { get; init; }
    }

    private sealed class VoidOutcome : Outcome
    {
    }
}

public static class ResultExtensions
{
    public static Task<Outcome> Completed<T>(this T result) where T : Outcome => Task.FromResult<Outcome>(result);
}
