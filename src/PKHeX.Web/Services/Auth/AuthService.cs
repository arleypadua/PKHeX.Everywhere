using Microsoft.JSInterop;

namespace PKHeX.Web.Services.Auth;

public class AuthService(
    IJSRuntime js)
{
    public async Task<string> GetAuthToken()
    {
        var authToken = await js.InvokeAsync<string>("getAuthToken");
        return authToken;
    }
}