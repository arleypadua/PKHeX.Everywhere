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
    private readonly FixedSizeQueue<Failure> _failures = new(20);
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
            var showMessage = message.Error(RenderErrorMessage());
            showMessage.Start();
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
        navigation.NavigateToPlugInPage(plugin.Id, goToPage.Path);
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

internal sealed class FixedSizeQueue<T>(int maxSize)
{
    private readonly Queue<T> _queue = new();

    public void Enqueue(T item)
    {
        if (_queue.Count >= maxSize)
        {
            _queue.Dequeue();
        }
        _queue.Enqueue(item);
    }

    public IEnumerable<T> GetItems()
    {
        return _queue.ToArray();
    }
}