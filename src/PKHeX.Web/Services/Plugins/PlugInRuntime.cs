using AntDesign;
using Blazor.Analytics;
using Microsoft.AspNetCore.Components;
using PKHeX.Web.Extensions;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public partial class PlugInRuntime(
    PlugInRegistry registry, 
    PlugInPageRegistry pageRegistry,
    IMessageService message,
    NavigationManager navigation,
    INotificationService notificationService,
    AnalyticsService analyticsService)
{
    private readonly FixedSizeList<Failure> _failures = new(20);
    public IEnumerable<Failure> RecentFailures => _failures.GetItems();
        
    public async Task RunAll<T>(Func<T, Task<Outcome>> action) where T : IPluginHook
    {
        var failed = false;
        var hooks = registry.GetAllEnabledHooks<T>();
        foreach (var hook in hooks)
        {
            try
            {
                var outcome = await action(hook);
                Handle(hook, outcome);
                Track(hook);
            }
            catch (Exception e)
            {
                failed = true;
                _failures.Enqueue(new (registry.GetPlugInOwningHook(hook), e));
                Track(hook, e);
            }
        }

        if (failed)
        {
            _ = message.Error(RenderErrorMessage());
        }
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
        var plugin = registry.GetPlugInOwningHook(hook);
        pageRegistry.Register(plugin.Id, goToPage);
        navigation.NavigateToPlugInPage(plugin.Id, goToPage.Path, goToPage.Layout);
        return Task.CompletedTask;
    }

    private Task HandleNotification(Outcome.Notification notification)
    {
        return notificationService.Open(new()
        {
            Message = notification.Message,
            Description = notification.Description,
            NotificationType = (NotificationType)notification.Type,
        });
    }
    
    private void Track(IPluginHook hook, Exception? failure = null)
    {
        analyticsService.TrackPlugInHookExecuted(hook, failure);
    }

    public record Failure(LoadedPlugIn PlugIn, Exception Exception);
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