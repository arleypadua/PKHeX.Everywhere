using AntDesign;
using Blazored.LocalStorage;

namespace PKHeX.Web.Services;

public class AntdThemeService
{
    private readonly BrowserWindowService.Instance _windowsService;
    private readonly ISyncLocalStorageService _localStorage;

    public AntdThemeService(BrowserWindowService.Instance windowsService,
        ISyncLocalStorageService localStorage)
    {
        _windowsService = windowsService;
        _localStorage = localStorage;

        Theme = GetTheme();
    }
    
    public static event Func<GlobalTheme, Task>? ThemeChanged;

    public GlobalTheme Theme { get; private set; }
    public bool IsDarkTheme => Theme == GlobalTheme.Dark;
    
    public readonly Dictionary<string, int> ColumnsConfiguration = new()
    {
        { "Xxl", 3 },
        { "Xl", 3 },
        { "Lg", 3 },
        { "Md", 3 },
        { "Sm", 2 },
        { "Xs", 1 }
    };

    public async Task SetDarkTheme()
    {
        Theme = GlobalTheme.Dark;
        _localStorage.SetItemAsString("theme", GlobalTheme.Dark.Name);
        
        if (ThemeChanged != null)
            await ThemeChanged.Invoke(Theme);
    }

    public async Task SetLightTheme()
    {
        Theme = GlobalTheme.Light;
        _localStorage.SetItemAsString("theme", GlobalTheme.Light.Name);
        
        if (ThemeChanged != null)
            await ThemeChanged.Invoke(Theme);
    }

    private GlobalTheme GetTheme()
    {
        var theme = _localStorage.GetItemAsString("theme");
        if (theme is null) return GetThemeFromUserPreference();
        
        return theme switch
        {
            "light" => GlobalTheme.Light,
            "dark" => GlobalTheme.Dark,
            "compact" => GlobalTheme.Compact,
            "aliyun" => GlobalTheme.Aliyun,
            _ => throw new ArgumentOutOfRangeException("Theme not supported")
        };
    }

    private GlobalTheme GetThemeFromUserPreference() => _windowsService.HasPreferenceForDarkTheme 
        ? GlobalTheme.Dark 
        : GlobalTheme.Light;
}