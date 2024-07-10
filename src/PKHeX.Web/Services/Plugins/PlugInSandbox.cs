using System.Reflection;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public class PlugInSandbox
{
    private readonly IServiceProvider _appServiceProvider;
    private readonly HttpClient _httpClientFactory;
    private readonly Dictionary<string, Assembly> _pluginsRegistry = new();

    private ServiceProvider _pluginServiceProvider = null!;


    public PlugInSandbox(
        HttpClient httpClientFactory,
        IServiceProvider appServiceProvider)
    {
        _appServiceProvider = appServiceProvider;
        _httpClientFactory = httpClientFactory;

        RefreshServiceProvider();
    }

    public async Task LoadFrom(string url)
    {
        var assembly = await _httpClientFactory.GetByteArrayAsync(url);
        LoadAssembly(assembly);
    }

    public IEnumerable<Settings> GetAllSettings() => _pluginServiceProvider.AllSettings();
    public IEnumerable<IPluginHook> GetAllHooksOf(Settings plugin)
    {
        var assembly = plugin.GetType().Assembly;
        var types = new List<Type>();
        types.AddRange(assembly.GetConcreteTypesOf<IRunOnPokemonChange>());
        types.AddRange(assembly.GetConcreteTypesOf<IRunOnPokemonSave>());
        types.AddRange(assembly.GetConcreteTypesOf<IRunOnItemChanged>());
        
        return types.Select(t => _pluginServiceProvider.GetFromImplementation(t) as IPluginHook)!;
    }

    public IEnumerable<T> GetAllHooks<T>() where T : IPluginHook => _pluginServiceProvider.GetServices<T>();
    public IEnumerable<T> GetAllEnabledHooks<T>() where T : IPluginHook => _pluginServiceProvider.GetServices<T>()
        .Where(h => GetSettingsOwningPlugin(h).Enabled);

    private Settings GetSettingsOwningPlugin<T>(T hook) where T : IPluginHook =>
        _pluginServiceProvider.GetRequiredKeyedService<Settings>(hook.GetType().Assembly.SettingKeyName());

    private void LoadAssembly(byte[] assemblyBytes)
    {
        if (assemblyBytes.Length == 0) return;

        var assembly = Assembly.Load(assemblyBytes);
        _pluginsRegistry[assembly.GetName().FullName] = assembly;

        RefreshServiceProvider(_pluginServiceProvider);
    }

    private void RefreshServiceProvider(ServiceProvider? existing = null)
    {
        existing?.Dispose();
        _pluginServiceProvider = BuildServiceCollection().BuildServiceProvider();
    }

    private IServiceCollection BuildServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGameProvider>(
            new GameServiceProxy(_appServiceProvider.GetRequiredService<GameService>()));

        return services
            .RegisterPluginAt(_pluginsRegistry.Values);
    }
}

internal static class ServiceCollectionExtensions
{
    internal static IEnumerable<Settings> AllSettings(this IServiceProvider serviceProvider)
    {
        var services = serviceProvider.GetService<IServiceCollection>();
        return services?
            .Where(s => s.IsKeyedService)
            .Select(s => s.KeyedImplementationInstance)
            .OfType<Settings>() ?? [];
    }
    
    internal static object GetFromImplementation(this IServiceProvider serviceProvider, Type implementation)
    {
        var services = serviceProvider.GetService<IServiceCollection>();
        var byImplementation = services?
            .FirstOrDefault(s => !s.IsKeyedService && s.ImplementationType == implementation);
        
        if (byImplementation == null) return Array.Empty<Settings>();

        return serviceProvider.GetRequiredService(byImplementation.ServiceType);
    }

    internal static IServiceCollection RegisterPluginAt(this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            services.RegisterPluginAt(assembly);
        }

        return services;
    }

    private static void RegisterPluginAt(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton(services);
        
        var onPokemonChangeTypes = assembly.GetConcreteTypesOf<IRunOnPokemonChange>();
        foreach (var type in onPokemonChangeTypes) services.AddTransient(typeof(IRunOnPokemonChange), type);

        var onPokemonSaveTypes = assembly.GetConcreteTypesOf<IRunOnPokemonSave>();
        foreach (var type in onPokemonSaveTypes) services.AddTransient(typeof(IRunOnPokemonSave), type);

        var onItemChangedTypes = assembly.GetConcreteTypesOf<IRunOnItemChanged>();
        foreach (var type in onItemChangedTypes) services.AddTransient(typeof(IRunOnItemChanged), type);

        var settings = assembly.GetSettings();
        services.AddKeyedSingleton(assembly.SettingKeyName(), settings);
    }

    internal static string SettingKeyName(this Assembly assembly) => $"[Setting].[{assembly.GetName().FullName}]";

    internal static IEnumerable<Type> GetConcreteTypesOf<TInterface>(this Assembly assembly)
        where TInterface : IPluginHook => assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(TInterface)))
        .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false });

    private static Settings GetSettings(this Assembly assembly) => assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(Settings)))
        .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false })
        .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
        .Take(1)
        .Select(t => (Settings?)Activator.CreateInstance(t))
        .SingleOrDefault() ?? throw new InvalidOperationException(
        "One class implementing PKHeX.Web.Plugins.Settings could not be found. Make sure you have exactly one class implementing it with a parameterless constructor.");
}

public static class PluginSandboxExtensions
{
    public static async Task OnPokemonChanged(this PlugInSandbox sandbox, Pokemon pokemon)
    {
        var hooks = sandbox.GetAllEnabledHooks<IRunOnPokemonChange>();
        foreach (var hook in hooks)
        {
            // consciously running one after the other since the whole pokemon api is mutable
            // and we don't want one plugin stepping on the foot of the other
            await hook.OnPokemonChange(pokemon);
        }
    }

    public static async Task OnPokemonSaved(this PlugInSandbox sandbox, Pokemon pokemon)
    {
        var hooks = sandbox.GetAllEnabledHooks<IRunOnPokemonSave>();
        foreach (var hook in hooks)
        {
            // consciously running one after the other since the whole pokemon api is mutable
            // and we don't want one plugin stepping on the foot of the other
            await hook.OnPokemonSaved(pokemon);
        }
    }

    public static async Task OnItemChanged(this PlugInSandbox sandbox, ushort id, uint count)
    {
        var hooks = sandbox.GetAllEnabledHooks<IRunOnItemChanged>();
        foreach (var hook in hooks)
        {
            // consciously running one after the other since the whole pokemon api is mutable
            // and we don't want one plugin stepping on the foot of the other
            await hook.OnItemChanged(new IRunOnItemChanged.ItemChanged(id, count));
        }
    }
}