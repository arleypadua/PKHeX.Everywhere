using Microsoft.JSInterop;

namespace PKHeX.Web.Services.Auth;

public class AuthService(
    IJSRuntime js)
{
    public static event Func<IdTokenChangedArgs, Task>? TokenChanged;
    
    private IJSInProcessRuntime SyncJs => js as IJSInProcessRuntime ??
                                          throw new NotSupportedException(
                                              "Requested an in process javascript interop, but none was found");

    public bool IsEnabled() => InvokeSync("isFirebaseAuthEnabled", false);
    
    public bool IsSignedIn()
        => InvokeSync("isSignedIn", false);
    
    public async Task<string> SignInAnonymously()
        => await InvokeAsync("signInAnonymously", string.Empty);
    
    public async Task<string> GetAuthToken()
        => await InvokeAsync("getAuthToken", string.Empty);
    
    public async Task<User?> GetSignedInUser()
        => await InvokeAsync<User?>("getSignedInUser", null);

    private bool InvokeSync(string identifier, bool fallback)
    {
        try
        {
            return SyncJs.Invoke<bool>(identifier);
        }
        catch (Exception ex) when (IsInteropUnavailable(ex))
        {
            return fallback;
        }
    }

    private async Task<T> InvokeAsync<T>(string identifier, T fallback)
    {
        try
        {
            return await js.InvokeAsync<T>(identifier);
        }
        catch (Exception ex) when (IsInteropUnavailable(ex))
        {
            return fallback;
        }
    }

    private static bool IsInteropUnavailable(Exception ex)
        => ex is JSException or InvalidOperationException or NotSupportedException;
    
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

public static class AuthServiceExtensions
{
    public static async Task<AuthService.User> GetSignedInUserOrThrow(this AuthService auth)
    {
        var user = await auth.GetSignedInUser();
        return user ?? throw new InvalidOperationException("User is not signed in");
    }
}