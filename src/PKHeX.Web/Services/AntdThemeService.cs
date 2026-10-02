using AntDesign;
using Microsoft.JSInterop;

namespace PKHeX.Web.Services;

public class AntdThemeService(ReactApp reactApp)
{
    private DotNetObjectReference<AntdThemeService>? _self;

    public static event Func<GlobalTheme, Task>? ThemeChanged;

    public GlobalTheme Theme { get; private set; } = GlobalTheme.Light;

    public readonly Dictionary<string, int> ColumnsConfiguration = new()
    {
        { "Xxl", 3 },
        { "Xl", 3 },
        { "Lg", 3 },
        { "Md", 3 },
        { "Sm", 2 },
        { "Xs", 1 }
    };

    public async Task Load()
    {
        try
        {
            Theme = ToTheme(await reactApp.InvokeAsync<string>("getTheme"));
            _self ??= DotNetObjectReference.Create(this);
            await reactApp.InvokeVoidAsync("onThemeChanged", _self);
        }
        catch (JSException)
        {
            // App renders nothing until the theme loads, so a missing React bundle must not leave it blank.
        }
    }

    [JSInvokable]
    public async Task OnThemeChanged(string theme)
    {
        Theme = ToTheme(theme);

        if (ThemeChanged != null)
            await ThemeChanged.Invoke(Theme);
    }

    private static GlobalTheme ToTheme(string theme) => theme == "dark" ? GlobalTheme.Dark : GlobalTheme.Light;
}
