using Microsoft.JSInterop;

namespace PKHeX.Web.Services.Auth;

public class AuthService(
    IJSRuntime js)
{
    public static event Func<IdTokenChangedArgs, Task>? TokenChanged;
    
    private IJSInProcessRuntime SyncJs => js as IJSInProcessRuntime ??
                                          throw new NotSupportedException(
                                              "Requested an in process javascript interop, but none was found");
    
    public bool IsSignedIn()
    {
        var isSignedId = SyncJs.Invoke<bool>("isSignedIn");
        return isSignedId;
    }
    
    public async Task<string> SignInAnonymously()
    {
        var authToken = await js.InvokeAsync<string>("signInAnonymously");
        return authToken;
    }
    
    public async Task<string> GetAuthToken()
    {
        var authToken = await js.InvokeAsync<string>("getAuthToken");
        return authToken;
    }
    
    public async Task<User> GetSignedInUser()
    {
        return await js.InvokeAsync<User>("getSignedInUser");
    }
    
    [JSInvokable]
    public static async Task OnTokenChanged(string? token)
    {
        var task = TokenChanged?.Invoke(new IdTokenChangedArgs(token));
        if (task is not null)
            await task;
    }
    
    public record User(
        string Id,
        string? Email,
        bool IsAnonymous);

    public record IdTokenChangedArgs(
        string? Token)
    {
        public bool IsSignedIn => !string.IsNullOrEmpty(Token);
    }
}