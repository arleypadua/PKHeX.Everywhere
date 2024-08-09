using System.Reflection;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

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

    internal static object? GetFromImplementation(this IServiceProvider serviceProvider, Type implementation)
    {
        var services = serviceProvider.GetService<IServiceCollection>();
        var byImplementation = services?
            .FirstOrDefault(s => !s.IsKeyedService && s.ImplementationType == implementation);

        if (byImplementation == null) return null;

        return serviceProvider
            .GetServices(byImplementation.ServiceType)
            .FirstOrDefault(s => s?.GetType() == implementation);
    }

    internal static IServiceCollection RegisterPluginAt(this IServiceCollection services,
        IEnumerable<LoadedPlugIn> plugins)
    {
        foreach (var plugin in plugins)
        {
            services.RegisterPlugin(plugin);
        }

        return services;
    }

    private static void RegisterPlugin(this IServiceCollection services, LoadedPlugIn plugin)
    {
        services.AddSingleton(services);

        foreach (var type in plugin.Hooks.Implementing<IQuickAction>())
            services.AddTransient(typeof(IQuickAction), type);

        foreach (var type in plugin.Hooks.Implementing<IRunOnPokemonChange>())
            services.AddTransient(typeof(IRunOnPokemonChange), type);

        foreach (var type in plugin.Hooks.Implementing<IRunOnPokemonSave>())
            services.AddTransient(typeof(IRunOnPokemonSave), type);

        foreach (var type in plugin.Hooks.Implementing<IRunOnItemChanged>())
            services.AddTransient(typeof(IRunOnItemChanged), type);
        
        foreach (var type in plugin.Hooks.Implementing<IPokemonEditAction>())
            services.AddTransient(typeof(IPokemonEditAction), type);
        
        foreach (var type in plugin.Hooks.Implementing<IPokemonStatsEditAction>())
            services.AddTransient(typeof(IPokemonStatsEditAction), type);

        services.AddKeyedSingleton(plugin.Assembly.SettingKeyName(), plugin.Settings);
        services.AddSingleton(plugin.Settings.GetType(), plugin.Settings);
    }
}

public static class AssemblyExtensions
{
    internal static string SettingKeyName(this Assembly assembly) => $"[Setting].[{assembly.GetName().FullName}]";

    internal static IEnumerable<Type> GetConcreteTypesOf<TInterface>(this Assembly assembly)
        where TInterface : IPluginHook => assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(TInterface)))
        .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false });

    public static Settings GetSettings(this Assembly assembly) => assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(Settings)))
        .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false })
        .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
        .Take(1)
        .Select(t => (Settings?)Activator.CreateInstance(t))
        .SingleOrDefault() ?? throw new InvalidOperationException(
        "One class implementing PKHeX.Web.Plugins.Settings could not be found. Make sure you have exactly one class implementing it with a parameterless constructor.");
}

public static class TypeExtensions
{
    public static IEnumerable<Type> Implementing<T>(this IEnumerable<Type> types) => types
        .Where(t => t.IsAssignableTo(typeof(T)))
        .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false });

    public static string GetFullNameOrName(this Type type) => type.FullName ?? type.Name;
}