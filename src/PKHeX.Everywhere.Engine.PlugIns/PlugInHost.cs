using System.Reflection;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.PlugIns;

public sealed class PlugInHost(Session session)
{
    private readonly Dictionary<string, RegisteredPlugIn> _plugIns = new();
    private readonly IGameProvider _game = new SessionGameProvider(session);

    public event Action<PlugInRan>? Ran;

    public static PlugInSdk DetectSdk(byte[] assembly) => PlugInSdkDetector.Detect(assembly);

    public RegisteredPlugIn Register(byte[] assembly, StoredPlugIn? stored = null)
    {
        var sdk = DetectSdk(assembly);
        if (sdk != PlugInSdk.V2) throw new IncompatiblePlugInException(sdk);

        var loaded = Assembly.Load(assembly);
        var plugIn = new RegisteredPlugIn(loaded, CreateSettings(loaded), _game);
        if (stored is not null) Restore(plugIn, stored);

        Unregister(plugIn.Id);
        _plugIns[plugIn.Id] = plugIn;
        return plugIn;
    }

    public void Unregister(string id)
    {
        if (_plugIns.Remove(id, out var plugIn)) plugIn.Dispose();
    }

    public IReadOnlyList<RegisteredPlugIn> List() => _plugIns.Values.ToList();

    public RegisteredPlugIn? Find(string id) => _plugIns.GetValueOrDefault(id);

    public void SetEnabled(string id, bool enabled) => Get(id).Enabled = enabled;

    public void SetToggle(string id, string hookId, bool enabled) => Get(id).SetToggle(hookId, enabled);

    public void UpdateSetting(string id, string key, Settings.SettingValue value) => Get(id).Settings[key] = value;

    public async Task RunAll<THook>(Func<THook, Task<Outcome>> run) where THook : IPluginHook
    {
        var hooks = _plugIns.Values
            .Where(p => p.Enabled)
            .SelectMany(p => p.EnabledHooksOf<THook>(), (p, h) => (PlugInId: p.Id, HookId: h.Id, h.Hook))
            .ToList();

        foreach (var (plugInId, hookId, hook) in hooks)
        {
            try
            {
                var outcome = await run(hook);
                Ran?.Invoke(new PlugInRan(plugInId, hookId, outcome, null));
            }
            catch (Exception e)
            {
                Ran?.Invoke(new PlugInRan(plugInId, hookId, null, e));
            }
        }

        if (hooks.Count > 0) session.Invalidate(Topics.All);
    }

    private RegisteredPlugIn Get(string id) =>
        _plugIns.GetValueOrDefault(id) ?? throw new KeyNotFoundException($"Plug-in {id} is not registered.");

    private static Settings CreateSettings(Assembly assembly) => assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(Settings)))
        .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
        .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
        .Select(t => (Settings)Activator.CreateInstance(t)!)
        .FirstOrDefault() ?? throw new InvalidOperationException(
        $"{assembly.GetName().Name} needs exactly one class extending {typeof(Settings).FullName} with a parameterless constructor.");

    private static void Restore(RegisteredPlugIn plugIn, StoredPlugIn stored)
    {
        plugIn.Enabled = stored.Enabled;
        foreach (var (hookId, enabled) in stored.Toggles) plugIn.SetToggle(hookId, enabled);
        foreach (var (key, value) in stored.Settings)
        {
            var current = plugIn.Settings.GetOrDefault(key);
            if (current is { ReadOnly: true } || current is not null && current.GetType() != value.GetType()) continue;
            plugIn.Settings[key] = value;
        }
    }

    private sealed class SessionGameProvider(Session session) : IGameProvider
    {
        public Game? Game => session.Game;
        public string? FileName => session.FileName;
        public bool IsLoaded => session.Game is not null;
    }
}

public sealed record StoredPlugIn(
    bool Enabled,
    IReadOnlyDictionary<string, bool> Toggles,
    IReadOnlyDictionary<string, Settings.SettingValue> Settings);

public sealed record PlugInRan(string PlugInId, string HookId, Outcome? Outcome, Exception? Failure);
