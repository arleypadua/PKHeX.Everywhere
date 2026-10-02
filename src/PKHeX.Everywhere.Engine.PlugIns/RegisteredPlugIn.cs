using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.PlugIns;

public sealed class RegisteredPlugIn
{
    private readonly ServiceProvider _services;
    private readonly IReadOnlyList<Type> _hookTypes;
    private readonly Dictionary<string, bool> _toggles = new();

    internal RegisteredPlugIn(Assembly assembly, Settings settings, IGameProvider game)
    {
        Assembly = assembly;
        Settings = settings;
        _hookTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IPluginHook)))
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
            .ToList();

        var services = new ServiceCollection()
            .AddSingleton(game)
            .AddSingleton(settings)
            .AddSingleton(settings.GetType(), settings);
        foreach (var hookType in _hookTypes) services.AddTransient(hookType);
        _services = services.BuildServiceProvider();

        foreach (var (hookType, enabled) in settings.DefaultFeatureToggles) _toggles[HookIdOf(hookType)] = enabled;
    }

    public Assembly Assembly { get; }
    public string Id => Assembly.GetName().Name!;
    public Version Version => Assembly.GetName().Version!;
    public Settings Settings { get; }
    public bool Enabled { get; internal set; } = true;

    public IEnumerable<string> HookIds => _hookTypes.Select(HookIdOf);

    public IReadOnlyList<PlugInHook> Hooks => _hookTypes
        .Select(t => new PlugInHook(HookIdOf(t), Create(t).Description, IsHookEnabled(HookIdOf(t))))
        .ToList();

    public bool IsHookEnabled(string hookId) => _toggles.GetValueOrDefault(hookId);

    internal void SetToggle(string hookId, bool enabled) => _toggles[hookId] = enabled;

    internal IEnumerable<(string Id, T Hook)> EnabledHooksOf<T>() where T : IPluginHook => _hookTypes
        .Where(t => t.IsAssignableTo(typeof(T)) && IsHookEnabled(HookIdOf(t)))
        .Select(t => (HookIdOf(t), (T)Create(t)));

    internal string? ReadModule(string module)
    {
        using var stream = Assembly.GetManifestResourceStream(module);
        if (stream is null) return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal void Dispose() => _services.Dispose();

    private IPluginHook Create(Type hookType) => (IPluginHook)_services.GetRequiredService(hookType);

    private static string HookIdOf(Type hookType) => hookType.FullName ?? hookType.Name;
}

/// <summary>
/// One hook of a plug-in: an action or an event handler it adds.
/// </summary>
/// <param name="Id">The hook's id, the full name of its .NET type.</param>
/// <param name="Enabled">Whether the hook is on. Hooks start off unless the plug-in turns them on by default.</param>
public sealed record PlugInHook(string Id, string Description, bool Enabled);
