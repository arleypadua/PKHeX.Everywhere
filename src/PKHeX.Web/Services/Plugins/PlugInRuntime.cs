using AntDesign;
using Microsoft.AspNetCore.Components;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public partial class PlugInRuntime(
    PlugInRegistry registry, 
    IMessageService message,
    NavigationManager navigation,
    INotificationService notificationService)
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
                Handle(outcome);
            }
            catch (Exception e)
            {
                failed = true;
                _failures.Enqueue(new (registry.GetPlugInOwningHook(hook), e));
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
        var outcome = await action(hook);
        Handle(outcome);
    }

    private void Handle(Outcome outcome) => _ = outcome switch
    {
        Outcome.Notification notification => HandleNotification(notification),
        _ => Task.CompletedTask
    };

    private Task HandleNotification(Outcome.Notification notification)
    {
        Console.WriteLine("result completed");
        return notificationService.Open(new()
        {
            Message = notification.Message,
            Description = notification.Description,
            NotificationType = (NotificationType)notification.Type,
        });
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