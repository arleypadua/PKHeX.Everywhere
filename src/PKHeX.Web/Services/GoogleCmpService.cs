using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace PKHeX.Web.Services;

public class GoogleCmpService(IJSRuntime js, ISyncLocalStorageService localStorage)
{
    public bool IsEnabled => localStorage.GetItemAsString("google-cmp") == "on";

    public ValueTask ShowRevocationMessage() => js.InvokeVoidAsync("showGoogleCmpRevocationMessage");
}
