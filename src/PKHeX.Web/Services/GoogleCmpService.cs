using Microsoft.JSInterop;

namespace PKHeX.Web.Services;

public class GoogleCmpService(IJSRuntime js)
{
    private IJSInProcessRuntime SyncJs => js as IJSInProcessRuntime ??
                                          throw new NotSupportedException(
                                              "Requested an in process javascript interop, but none was found");

    public bool IsEnabled => SyncJs.Invoke<string?>("localStorage.getItem", "google-cmp") == "on";

    public ValueTask ShowRevocationMessage() => js.InvokeVoidAsync("showGoogleCmpRevocationMessage");
}
