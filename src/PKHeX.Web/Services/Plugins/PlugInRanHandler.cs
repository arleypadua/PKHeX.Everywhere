using AntDesign;
using Microsoft.AspNetCore.Components;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Web.Extensions;

namespace PKHeX.Web.Services.Plugins;

public sealed class PlugInRanHandler : IDisposable
{
    private readonly PlugInHost _host;
    private readonly NavigationManager _navigation;
    private readonly INotificationService _notificationService;
    private readonly AnalyticsService _analyticsService;

    public PlugInRanHandler(
        PlugInHost host,
        NavigationManager navigation,
        INotificationService notificationService,
        AnalyticsService analyticsService)
    {
        _host = host;
        _navigation = navigation;
        _notificationService = notificationService;
        _analyticsService = analyticsService;
        _host.Ran += HandleRan;
    }

    private void HandleRan(PlugInRan ran)
    {
        _analyticsService.TrackPlugInHookExecuted(ran.HookId.Split('.', '+').Last(), ran.Failure);

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
            case Outcome.Notification notification:
                _ = _notificationService.Open(new NotificationConfig
                {
                    Message = notification.Message,
                    Description = notification.Description,
                    NotificationType = (NotificationType)notification.Type,
                });
                break;
            case Outcome.PageRequest request:
                OpenPage(ran.PlugInId, request.Path);
                break;
        }
    }

    private void OpenPage(string plugInId, string path)
    {
        var page = _host.Pages().FirstOrDefault(p => p.PlugInId == plugInId && p.Path == path);
        if (page is not null) _navigation.NavigateToPlugInPage(plugInId, page.Path, page.Layout.ToString());
    }

    public void Dispose() => _host.Ran -= HandleRan;
}
