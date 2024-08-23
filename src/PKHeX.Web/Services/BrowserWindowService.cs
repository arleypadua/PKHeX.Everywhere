using Microsoft.JSInterop;

namespace PKHeX.Web.Services;

public static class BrowserWindowService
{
    public static event Func<ResizedArgs, Task>? WindowResized;
    public static event Func<SaveLoadedRequested, Task>? LoadRequested;
    
    [JSInvokable]
    public static async Task OnWindowResized(int width)
    {
        var task = WindowResized?.Invoke(new ResizedArgs(width));
        if (task is not null)
            await task;
    }

    [JSInvokable]
    public static async Task OnLoadRequested(SaveLoadedRequested message)
    {
        var task = LoadRequested?.Invoke(message);
        if (task is not null)
            await task;
    }

    public record ResizedArgs(int Width);

    public record SaveLoadedRequested(byte[] Bytes, string FileName);

    public class Instance(IJSRuntime jsRuntime)
    {
        private IJSInProcessRuntime Js => jsRuntime as IJSInProcessRuntime ??
                                              throw new NotSupportedException(
                                                  "Requested an in process javascript interop, but none was found");
        
        public int Width => Js.Invoke<int>("getWidth");

        public bool IsTooSmall => Width < 550;
        
        public bool HasPreferenceForDarkTheme => Js.Invoke<bool>("hasPreferenceForDarkTheme");
    }
}