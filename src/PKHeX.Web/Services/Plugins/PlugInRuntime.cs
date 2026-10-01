using AntDesign;
using Blazor.Analytics;
using Microsoft.AspNetCore.Components;
using PKHeX.Everywhere.Engine;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Web.Extensions;
using PKHeX.Web.Plugins;
using V2 = PKHeX.Everywhere.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public partial class PlugInRuntime : IDisposable
{
    private readonly PlugInRegistry _registry;
    private readonly PlugInPageRegistry _pageRegistry;
    private readonly NavigationManager _navigation;
    private readonly INotificationService _notificationService;
    private readonly AnalyticsService _analyticsService;
    private readonly Session _session;
    private readonly PlugInHost _host;
    private readonly FixedSizeList<Failure> _failures = new(20);

    public PlugInRuntime(
        PlugInRegistry registry,
        PlugInPageRegistry pageRegistry,
        NavigationManager navigation,
        INotificationService notificationService,
        AnalyticsService analyticsService,
        Session session,
        PlugInHost host)
    {
        _registry = registry;
        _pageRegistry = pageRegistry;
        _navigation = navigation;
        _notificationService = notificationService;
        _analyticsService = analyticsService;
        _session = session;
        _host = host;
        _host.Ran += HandleRan;
    }

    public IEnumerable<Failure> RecentFailures => _failures.GetItems();
        
    public async Task RunAll<T>(Func<T, Task<Outcome>> action) where T : IPluginHook
    {
        var ran = false;
        var failed = false;
        var hooks = _registry.GetAllEnabledHooks<T>();
        foreach (var hook in hooks)
        {
            ran = true;
            try
            {
                var outcome = await action(hook);
                Handle(hook, outcome);
                Track(hook);
            }
            catch (Exception e)
            {
                failed = true;
                _failures.Enqueue(new (_registry.GetPlugInOwningHook(hook), e));
                Track(hook, e);
            }
        }

        if (ran) _session.Invalidate(Topics.All);

        if (failed) NotifyFailure();
    }

    private void NotifyFailure()
    {
        _ = _notificationService.Open(new NotificationConfig
        {
            Message = "Plugin failed to execute",
            Description = "Visit /plugins/errors for details.",
            NotificationType = NotificationType.Error,
        });
    }

    private void HandleRan(PlugInRan ran)
    {
        _analyticsService.TrackPlugInHookExecuted(ran.HookId.Split('.').Last(), ran.Failure);

        if (ran.Failure is not null)
        {
            if (_registry.GetByOrNull(ran.PlugInId) is { } plugIn) _failures.Enqueue(new(plugIn, ran.Failure));
            NotifyFailure();
            return;
        }

        _ = ran.Outcome switch
        {
            V2.Outcome.Notification notification => _notificationService.Open(new()
            {
                Message = notification.Message,
                Description = notification.Description,
                NotificationType = (NotificationType)notification.Type,
            }),
            V2.Outcome.PageRequest page => OpenPage(ran.PlugInId, page.Path),
            _ => Task.CompletedTask,
        };
    }

    private Task OpenPage(string plugInId, string path)
    {
        if (_registry.GetByOrNull(plugInId) is not HostPlugIn plugIn) return Task.CompletedTask;

        var page = plugIn.Registered.Settings.Pages.FirstOrDefault(p => p.Path == path);
        if (page is not null) _navigation.NavigateToPlugInPage(plugInId, page.Path, page.Layout.ToString());
        return Task.CompletedTask;
    }

    public async Task RunOn<T>(T hook, Func<T, Task<Outcome>> action) where T : IPluginHook
    {
        try
        {
            var outcome = await action(hook);
            Handle(hook, outcome);
            Track(hook);
        }
        catch (Exception e)
        {
            Track(hook, e);
            throw;
        }
        finally
        {
            _session.Invalidate(Topics.All);
        }
    }

    public void Dismiss(Failure failure)
    {
        _failures.Remove(failure);
    }

    private void Handle(IPluginHook hook, Outcome outcome) => _ = outcome switch
    {
        Outcome.Notification notification => HandleNotification(notification),
        Outcome.PlugInPage goToPage => HandleGoToPlugInPage(hook, goToPage),
        _ => Task.CompletedTask
    };

    private Task HandleGoToPlugInPage(IPluginHook hook, Outcome.PlugInPage goToPage)
    {
        var plugin = _registry.GetPlugInOwningHook(hook);
        _pageRegistry.Register(plugin.Id, goToPage);
        _navigation.NavigateToPlugInPage(plugin.Id, goToPage.Path, goToPage.Layout.ToString());
        return Task.CompletedTask;
    }

    private Task HandleNotification(Outcome.Notification notification)
    {
        return _notificationService.Open(new()
        {
            Message = notification.Message,
            Description = notification.Description,
            NotificationType = (NotificationType)notification.Type,
        });
    }
    
    private void Track(IPluginHook hook, Exception? failure = null)
    {
        _analyticsService.TrackPlugInHookExecuted(hook.GetType().Name, failure);
    }

    public void Dispose() => _host.Ran -= HandleRan;

    public record Failure(InstalledPlugIn PlugIn, Exception Exception);
}

internal sealed class FixedSizeList<T>(int maxSize)
{
    private readonly List<T> _list = new();

    public void Enqueue(T item)
    {
        if (_list.Count >= maxSize)
        {
            var last = _list.LastOrDefault();
            if (last is not null) _list.Remove(last);
        }
        
        _list.Add(item);
    }

    public IEnumerable<T> GetItems()
    {
        return _list.ToArray();
    }

    public void Remove(T item)
    {
        _list.Remove(item);
    }
}