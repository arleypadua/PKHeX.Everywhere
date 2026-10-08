using Microsoft.Playwright;

namespace PKHeX.Web.E2E.Infrastructure;

// Playwright can't drag a file in from outside the page, so the test builds the DataTransfer and dispatches the drag events itself.
public sealed class FileDrag(IJSHandle dataTransfer) : IAsyncDisposable
{
    public Task DropOnAsync(ILocator target) => target.DispatchEventAsync("drop", new { dataTransfer });

    public ValueTask DisposeAsync() => dataTransfer.DisposeAsync();
}
