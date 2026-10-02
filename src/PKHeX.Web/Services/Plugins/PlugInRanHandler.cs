using AntDesign;
using Microsoft.AspNetCore.Components;
using PKHeX.Everywhere.Engine;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Web.Extensions;

namespace PKHeX.Web.Services.Plugins;

public sealed class PlugInRanHandler : IDisposable
{
    private readonly Session _session;
    private readonly PlugInHost _host;
    private readonly NavigationManager _navigation;
    private readonly INotificationService _notificationService;

    public PlugInRanHandler(
        Session session,
        PlugInHost host,
        NavigationManager navigation,
        INotificationService notificationService)
    {
        _session = session;
        _host = host;
        _navigation = navigation;
        _notificationService = notificationService;
        _session.Published += HandlePublished;
    }

    private void HandlePublished(IEngineEvent engineEvent)
    {
        if (engineEvent is not PlugInRan ran) return;

        if (ran.Failure is not null)
        {
            _ = _notificationService.Open(new NotificationConfig
            {
                Message = "Plugin failed to execute",
                Description = "Visit /plugins/errors for details.",
                NotificationType = NotificationType.Error,
            });
            return;
        }

        switch (ran.Outcome)
        {
            case { Kind: PlugInOutcomeKind.Notify } notification:
                _ = _notificationService.Open(new NotificationConfig
                {
                    Message = notification.Message,
                    Description = notification.Description,
                    NotificationType = (NotificationType)(notification.Type ?? PlugInNotificationType.None),
                });
                break;
            case { Kind: PlugInOutcomeKind.OpenPage, Path: { } path }:
                OpenPage(ran.PlugInId, path);
                break;
        }
    }

    private void OpenPage(string plugInId, string path)
    {
        var page = _host.Pages().FirstOrDefault(p => p.PlugInId == plugInId && p.Path == path);
        if (page is not null) _navigation.NavigateToPlugInPage(plugInId, page.Path, page.Layout.ToString());
    }

    public void Dispose() => _session.Published -= HandlePublished;
}
