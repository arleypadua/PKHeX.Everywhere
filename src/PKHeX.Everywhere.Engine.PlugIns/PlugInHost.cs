using System.Reflection;
using System.Runtime.CompilerServices;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine.PlugIns;

public sealed class PlugInHost
{
    private const int MaxFailures = 20;
    private static readonly ConditionalWeakTable<Session, PlugInHost> Hosts = new();

    private readonly Session _session;
    private readonly Dictionary<string, RegisteredPlugIn> _plugIns = new();
    private readonly List<PlugInFailure> _failures = [];
    private readonly IGameProvider _game;
    private readonly AsyncLocal<bool> _runningHook = new();

    public PlugInHost(Session session)
    {
        _session = session;
        _game = new SessionGameProvider(session);
        Hosts.AddOrUpdate(session, this);
        session.AddHandlers(HandlerRegistry.TryInvoke);
        session.Published += HandlePublished;
    }

    public event Action<PlugInRan>? Ran;

    public IReadOnlyList<PlugInFailure> Failures => _failures.ToList();

    internal static PlugInHost Of(Session session) => Hosts.TryGetValue(session, out var host)
        ? host
        : throw new EngineException(ErrorCodes.Unexpected, "No plug-in host is attached to the session.");

    public static IReadOnlySet<int> SupportedSdks { get; } = new HashSet<int> { (int)PlugInSdk.V1, (int)PlugInSdk.V2 };

    public static PlugInSdk DetectSdk(byte[] assembly) => PlugInSdkDetector.Detect(assembly);

    public static PublishedVersion? NewestCompatible(IEnumerable<PublishedVersion> versions) => versions
        .Where(v => SupportedSdks.Contains(v.Sdk))
        .Where(v => Version.TryParse(v.Version, out _))
        .MaxBy(v => Version.Parse(v.Version));

    public static PublishedVersion? SdkUpdateFor(byte[] installed, IEnumerable<PublishedVersion> versions) =>
        NewestCompatible(versions) is { } newest && newest.Sdk > (int)DetectSdk(installed) ? newest : null;

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

    public void Dismiss(PlugInFailure failure)
    {
        if (_failures.Remove(failure)) _session.Invalidate(Topics.All);
    }

    public Task Handle(IEngineEvent engineEvent) => engineEvent switch
    {
        ItemChanged changed => RunAll<IRunOnItemChanged>(h => h.OnItemChanged(new((ushort)changed.ItemId, (uint)changed.Count))),
        PokemonChanged changed => RunOnPokemonAt<IRunOnPokemonChange>(changed.At, (h, p) => h.OnPokemonChange(p)),
        PokemonSaved saved => RunOnPokemonAt<IRunOnPokemonSave>(saved.At, (h, p) => h.OnPokemonSaved(p)),
        _ => Task.CompletedTask,
    };

    public Task PokemonChanged(Pokemon pokemon) => RunAll<IRunOnPokemonChange>(h => h.OnPokemonChange(pokemon));

    public Task PokemonSaved(Pokemon pokemon) => RunAll<IRunOnPokemonSave>(h => h.OnPokemonSaved(pokemon));

    public Task RunAll<THook>(Func<THook, Task<Outcome>> run) where THook : IPluginHook => Run(run);

    private async Task Run<THook>(Func<THook, Task<Outcome>> run, Action? afterHooks = null) where THook : IPluginHook
    {
        var hooks = _plugIns.Values
            .Where(p => p.Enabled)
            .SelectMany(p => p.EnabledHooksOf<THook>(), (p, h) => (PlugInId: p.Id, HookId: h.Id, h.Hook))
            .ToList();

        _runningHook.Value = true;
        foreach (var (plugInId, hookId, hook) in hooks)
        {
            PlugInRan ran;
            try
            {
                ran = new PlugInRan(plugInId, hookId, await run(hook), null);
            }
            catch (Exception e)
            {
                Record(new PlugInFailure(plugInId, hookId, e.Message, e.StackTrace));
                ran = new PlugInRan(plugInId, hookId, null, e);
            }

            Ran?.Invoke(ran);
        }

        if (hooks.Count == 0) return;

        afterHooks?.Invoke();
        _session.Invalidate(Topics.All);
    }

    // Events raised while a hook runs come from the hook's own writes, so running hooks for them could loop.
    private void HandlePublished(IEngineEvent engineEvent)
    {
        if (!_runningHook.Value) _ = Handle(engineEvent);
    }

    private async Task RunOnPokemonAt<THook>(PokemonHandle at, Func<THook, Pokemon, Task<Outcome>> run) where THook : IPluginHook
    {
        var slot = _session.RequireGame().Find(at);
        await Run<THook>(h => run(h, slot.Pokemon), slot.Save);
    }

    private void Record(PlugInFailure failure)
    {
        if (_failures.Count == MaxFailures) _failures.RemoveAt(0);
        _failures.Add(failure);
    }

    private RegisteredPlugIn Get(string id) =>
        _plugIns.GetValueOrDefault(id) ?? throw new KeyNotFoundException($"Plug-in {id} is not registered.");

    private static Settings CreateSettings(Assembly assembly) => assembly.GetTypes()
        .Where(t => t.IsAssignableTo(typeof(Settings)))
        .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
        .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
        .Select(t => (Settings)Activator.CreateInstance(t)!)
        .FirstOrDefault() ?? throw new InvalidOperationException(
        $"{assembly.GetName().Name} needs a class extending {typeof(Settings).FullName} with a parameterless constructor.");

    private static void Restore(RegisteredPlugIn plugIn, StoredPlugIn stored)
    {
        plugIn.Enabled = stored.Enabled;
        foreach (var (hookId, enabled) in stored.Toggles) plugIn.SetToggle(hookId, enabled);
        foreach (var (key, value) in stored.Settings)
        {
            var current = plugIn.Settings.GetOrDefault(key);
            if (current is null || current.ReadOnly || current.GetType() != value.GetType()) continue;
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

public sealed record PlugInFailure(string PlugInId, string HookId, string Message, string? StackTrace);
