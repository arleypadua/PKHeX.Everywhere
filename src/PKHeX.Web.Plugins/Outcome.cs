using Microsoft.AspNetCore.Components;

namespace PKHeX.Web.Plugins;

public abstract class Outcome
{
    // only allow this library to specify valid implementations
    private protected Outcome()
    {
    }

    public static readonly Outcome Void = new VoidOutcome();

    public static Notification Notify(string message, string? description = null,
        Notification.NotificationType type = Notification.NotificationType.None) => new Notification
    {
        Message = message,
        Description = description,
        Type = type
    };
    
    public static PlugInPage Page(string path, 
        Type pageComponentType, 
        PlugInPage.PageLayout layout = PlugInPage.PageLayout.Standard) => new ()
    {
        Path = path, 
        ComponentType = pageComponentType,
        Layout = layout,
    };
    
    public static PlugInPage Page<TPage>(string path, 
        PlugInPage.PageLayout layout = PlugInPage.PageLayout.Standard) where TPage : PkHexWebPlugInComponent => new ()
    {
        Path = path, 
        ComponentType = typeof(TPage),
        Layout = layout,
    };

    public class Notification : Outcome
    {
        public required string Message { get; set; }
        public string? Description { get; set; }
        public NotificationType Type { get; set; } = NotificationType.None;

        public enum NotificationType
        {
            None = 0,
            Info = 1,
            Success = 2,
            Warning = 3,
            Error = 4
        }
    }

    public class PlugInPage : Outcome
    {
        /// <summary>
        /// The URL path that this page responds to
        /// </summary>
        public required string Path { get; set; }
        
        /// <summary>
        /// The type of component to be rendered
        /// </summary>
        public required Type ComponentType { get; set; }

        /// <summary>
        /// The layout in which the plugin will be rendered at
        /// </summary>
        public required PageLayout Layout { get; set; }
        
        /// <summary>
        /// The page header to be rendered
        ///
        /// If null, no page header is present
        /// </summary>
        public Header? PageHeader { get; set; }

        /// <summary>
        /// Page header definition
        /// </summary>
        /// <param name="Title">The title of the page</param>
        /// <param name="ComponentType">The type of the component to be rendered</param>
        public record Header(string Title, Type? ComponentType);

        public enum PageLayout
        {
            Standard,
            Empty
        }
    }

    private class VoidOutcome : Outcome
    {
    }
}

public static class ResultExtensions
{
    public static Task<Outcome> Completed<T>(this T result) where T : Outcome => Task.FromResult<Outcome>(result);
}