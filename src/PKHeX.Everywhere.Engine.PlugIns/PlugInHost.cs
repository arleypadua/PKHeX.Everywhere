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
    private readonly HashSet<string> _needsReinstall = [];
    private readonly HashSet<string> _hasNewerVersion = [];
    private readonly List<PlugInFailure> _failures = [];
    private readonly IGameProvider _game;
    private readonly AsyncLocal<bool> _runningHook = new();
    private int _lastFailureId;

    public PlugInHost(Session session)
    {
        _session = session;
        _game = new SessionGameProvider(session);
        Hosts.AddOrUpdate(session, this);
        session.AddHandlers(HandlerRegistry.TryInvoke, HandlerRegistry.TryWriteEvent);
        session.Published += HandlePublished;
    }

    public IReadOnlyList<PlugInFailure> Failures => _failures.ToList();

    internal static PlugInHost Of(Session session) => Hosts.TryGetValue(session, out var host)
        ? host
        : throw new EngineException(ErrorCodes.Unexpected, "No plug-in host is attached to the session.");

    public static IReadOnlySet<int> SupportedSdks { get; } = new HashSet<int> { (int)PlugInSdk.V2 };

    public static PlugInSdk DetectSdk(byte[] assembly) => PlugInSdkDetector.Detect(assembly);

    public static bool IsSupported(byte[] assembly) => SupportedSdks.Contains((int)DetectSdk(assembly));

    public static PublishedVersion? NewestCompatible(IEnumerable<PublishedVersion> versions) => versions
        .Where(v => SupportedSdks.Contains(v.Sdk))
        .Where(v => Version.TryParse(v.Version, out _))
        .MaxBy(v => Version.Parse(v.Version));

    /// <summary>
    /// Registers a supported assembly with its stored state, or lists an unsupported one as needing reinstall.
    /// Registering a plug-in the host already knows is an update, and one without stored state is an install.
    /// An update without stored state keeps the state of the registered plug-in.
    /// </summary>
    public InstalledPlugIn Install(byte[] assembly, StoredPlugIn? stored = null)
    {
        var id = PlugInSdkDetector.NameOf(assembly)
                 ?? throw new IncompatiblePlugInException(PlugInSdk.None);
        var known = _plugIns.ContainsKey(id) || _needsReinstall.Contains(id);
        if (stored is null && _plugIns.ContainsKey(id)) stored = State(id) with { HasNewerVersion = false };

        if (!IsSupported(assembly))
        {
            if (_plugIns.Remove(id, out var replaced)) replaced.Dispose();
            _hasNewerVersion.Remove(id);
            _needsReinstall.Add(id);
            _session.Invalidate(Topics.All);
            return Installed(id);
        }

        var plugIn = Register(assembly, stored);
        var version = plugIn.Version.ToString();
        if (known) _session.Raise(new PlugInUpdated(plugIn.Id, version));
        else if (stored is null) _session.Raise(new PlugInInstalled(plugIn.Id, version));
        return Installed(plugIn.Id);
    }

    public IReadOnlyList<InstalledPlugIn> Installed() => _plugIns.Keys.Concat(_needsReinstall).Select(Installed).ToList();

    public bool HasNewerVersion(string id) => _hasNewerVersion.Contains(id);

    public PublishedVersion? NewestCompatible(string? id, IEnumerable<PublishedVersion> versions)
    {
        var newest = NewestCompatible(versions);
        if (id is null || Find(id) is not { } plugIn) return newest;

        var hasNewer = newest is not null && Version.Parse(newest.Version) > plugIn.Version;
        if (!(hasNewer ? _hasNewerVersion.Add(id) : _hasNewerVersion.Remove(id))) return newest;

        _session.AlsoWrote(Topics.PlugIns);
        _session.Invalidate(Topics.PlugIns);
        return newest;
    }

    public StoredPlugIn State(string id)
    {
        var plugIn = Get(id);
        return new StoredPlugIn(
            plugIn.Enabled,
            plugIn.HookIds.ToDictionary(h => h, plugIn.IsHookEnabled),
            plugIn.Settings.All.ToDictionary(),
            HasNewerVersion(id));
    }

    public RegisteredPlugIn Register(byte[] assembly, StoredPlugIn? stored = null)
    {
        var sdk = DetectSdk(assembly);
        if (!SupportedSdks.Contains((int)sdk)) throw new IncompatiblePlugInException(sdk);

        var loaded = Assembly.Load(assembly);
        var plugIn = new RegisteredPlugIn(loaded, CreateSettings(loaded), _game);
        if (stored is not null) Restore(plugIn, stored);

        if (_plugIns.Remove(plugIn.Id, out var replaced)) replaced.Dispose();
        _needsReinstall.Remove(plugIn.Id);
        if (stored?.HasNewerVersion == true) _hasNewerVersion.Add(plugIn.Id);
        else _hasNewerVersion.Remove(plugIn.Id);
        _plugIns[plugIn.Id] = plugIn;
        _session.Invalidate(Topics.All);
        return plugIn;
    }

    public void Unregister(string id)
    {
        _hasNewerVersion.Remove(id);
        var neededReinstall = _needsReinstall.Remove(id);
        if (_plugIns.Remove(id, out var plugIn)) plugIn.Dispose();
        else if (!neededReinstall) return;

        _session.Invalidate(Topics.All);
    }

    public IReadOnlyList<RegisteredPlugIn> List() => _plugIns.Values.ToList();

    public RegisteredPlugIn? Find(string id) => _plugIns.GetValueOrDefault(id);

    public void SetEnabled(string id, bool enabled)
    {
        Get(id).Enabled = enabled;
        _session.Invalidate(Topics.All);
    }

    public void SetToggle(string id, string hookId, bool enabled)
    {
        var plugIn = Get(id);
        if (!plugIn.HookIds.Contains(hookId)) throw new KeyNotFoundException($"Plug-in {id} has no hook {hookId}.");

        plugIn.SetToggle(hookId, enabled);
        _session.Invalidate(Topics.All);
    }

    public void UpdateSetting(string id, string key, Settings.SettingValue value)
    {
        var settings = Get(id).Settings;
        var current = settings.GetOrDefault(key) ?? throw new KeyNotFoundException($"Plug-in {id} has no setting {key}.");
        if (current.ReadOnly) throw new InvalidOperationException($"Setting {key} is read-only.");
        if (current.GetType() != value.GetType()) throw new InvalidOperationException($"Setting {key} takes a {current.GetType().Name}.");

        settings[key] = value with { ReadOnly = false };
        _session.Invalidate(Topics.All);
    }

    public IReadOnlyList<PlugInAction> Actions(ActionPlacement placement) => EnabledActions(placement)
        .Select(a => new PlugInAction(a.HookId, a.PlugInId, a.Label, a.Hook.Description, a.DisabledInfo.Disabled, a.DisabledInfo.Reason))
        .ToList();

    public async Task<PlugInRan> RunAction(string id, Pokemon? target = null)
    {
        _session.Game?.Require(Capability.PlugIns);
        ActionPlacement[] placements = target is null ? [ActionPlacement.Quick] : [ActionPlacement.Pokemon, ActionPlacement.PokemonStats];
        var action = placements.SelectMany(EnabledActions).FirstOrDefault(a => a.HookId == id)
                     ?? throw new EngineException(ErrorCodes.NotFound, $"No enabled plug-in action {id}.");

        if (action.DisabledInfo.Disabled)
            throw new EngineException(ErrorCodes.BadArguments, $"{action.Label} is disabled{(action.DisabledInfo.Reason is { } reason ? $": {reason}" : ".")}");

        var ran = await RunHook(action.PlugInId, action.HookId, () => action.Run(target));
        _session.Invalidate(Topics.All);
        return ran;
    }

    public IReadOnlyList<DeclaredPage> Pages() => EnabledPlugIns()
        .SelectMany(p => p.Settings.Pages, (p, page) => new DeclaredPage(p.Id, page.Path, page.Title, page.Layout))
        .ToList();

    public string PageModule(string plugInId, string path)
    {
        _session.Game?.Require(Capability.PlugIns);
        var plugIn = _plugIns.GetValueOrDefault(plugInId) is { Enabled: true } enabled
            ? enabled
            : throw new EngineException(ErrorCodes.NotFound, $"No enabled plug-in {plugInId}.");
        var page = plugIn.Settings.Pages.FirstOrDefault(p => p.Path == path)
                   ?? throw new EngineException(ErrorCodes.NotFound, $"{plugInId} has no page {path}.");

        return plugIn.ReadModule(page.Module)
               ?? throw new EngineException(ErrorCodes.NotFound, $"{plugInId} has no embedded module {page.Module}.");
    }

    public void Dismiss(int failureId)
    {
        if (_failures.RemoveAll(f => f.Id == failureId) > 0) _session.Invalidate(Topics.PlugIns);
    }

    public Task Handle(IEngineEvent engineEvent) => engineEvent switch
    {
        ItemChanged changed => RunAll<IRunOnItemChanged>(h => h.OnItemChanged(new((ushort)changed.ItemId, (uint)changed.Count))),
        PokemonChanged changed => RunOnPokemonAt<IRunOnPokemonChange>(changed.At, (h, p) => h.OnPokemonChange(p)),
        PokemonSaved saved => RunOnPokemonAt<IRunOnPokemonSave>(saved.At, (h, p) => h.OnPokemonSaved(p)),
        _ => Task.CompletedTask,
    };

    public Task RunAll<THook>(Func<THook, Task<Outcome>> run) where THook : IPluginHook => Run(run);

    private async Task Run<THook>(Func<THook, Task<Outcome>> run, Action? afterHooks = null) where THook : IPluginHook
    {
        var hooks = EnabledHooksOf<THook>().ToList();
        foreach (var (plugInId, hookId, hook) in hooks) await RunHook(plugInId, hookId, () => run(hook));

        if (hooks.Count == 0) return;

        afterHooks?.Invoke();
        _session.Invalidate(Topics.All);
    }

    private async Task<PlugInRan> RunHook(string plugInId, string hookId, Func<Task<Outcome>> run)
    {
        _runningHook.Value = true;
        PlugInRan ran;
        try
        {
            ran = new PlugInRan(plugInId, hookId, PlugInOutcome.From(await run()), null);
        }
        catch (Exception e)
        {
            Record(new PlugInFailure(++_lastFailureId, plugInId, hookId, e.Message, e.StackTrace));
            ran = new PlugInRan(plugInId, hookId, null, new HookFailure(e.GetType().Name, e.Message));
        }

        _session.PublishNow(ran);
        return ran;
    }

    private IEnumerable<RegisteredPlugIn> EnabledPlugIns() => _session.Game?.Supports(Capability.PlugIns) == false
        ? []
        : _plugIns.Values.Where(p => p.Enabled);

    private IEnumerable<(string PlugInId, string HookId, THook Hook)> EnabledHooksOf<THook>() where THook : IPluginHook => EnabledPlugIns()
        .SelectMany(p => p.EnabledHooksOf<THook>(), (p, h) => (p.Id, h.Id, h.Hook));

    private IEnumerable<EnabledAction> EnabledActions(ActionPlacement placement) => placement switch
    {
        ActionPlacement.Quick => EnabledHooksOf<IQuickAction>()
            .Select(h => new EnabledAction(h.PlugInId, h.HookId, h.Hook, h.Hook.Label, _ => h.Hook.OnActionRequested())),
        ActionPlacement.Pokemon => EnabledHooksOf<IPokemonEditAction>()
            .Select(h => new EnabledAction(h.PlugInId, h.HookId, h.Hook, h.Hook.Label, p => h.Hook.OnActionRequested(p!))),
        ActionPlacement.PokemonStats => EnabledHooksOf<IPokemonStatsEditAction>()
            .Select(h => new EnabledAction(h.PlugInId, h.HookId, h.Hook, h.Hook.Label, p => h.Hook.OnActionRequested(p!))),
        _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null),
    };

    private sealed record EnabledAction(string PlugInId, string HookId, IPluginHook Hook, string Label, Func<Pokemon?, Task<Outcome>> Run)
    {
        public IDisable.DisableInfo DisabledInfo => Hook is IDisable disable ? disable.DisabledInfo : IDisable.Enabled;
    }

    // Events raised while a hook runs come from the hook's own writes, so running hooks for them could loop.
    private void HandlePublished(IEngineEvent engineEvent)
    {
        if (!_runningHook.Value) _ = Handle(engineEvent);
    }

    private async Task RunOnPokemonAt<THook>(PokemonHandle at, Func<THook, Pokemon, Task<Outcome>> run) where THook : IPluginHook
    {
        var slot = _session.Find(at);
        await Run<THook>(h => run(h, slot.Pokemon), slot.Save);
    }

    private void Record(PlugInFailure failure)
    {
        if (_failures.Count == MaxFailures) _failures.RemoveAt(0);
        _failures.Add(failure);
    }

    private InstalledPlugIn Installed(string id) => Find(id) is { } plugIn
        ? new InstalledPlugIn(id, plugIn.Settings.Manifest.PlugInName, plugIn.Version.ToString(), plugIn.Enabled, HasNewerVersion(id), false)
        : new InstalledPlugIn(id, id, string.Empty, false, false, true);

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
    IReadOnlyDictionary<string, Settings.SettingValue> Settings,
    bool HasNewerVersion = false);

/// <summary>
/// A plug-in the engine has registered, as listed by <c>plugins.installed</c>.
/// </summary>
/// <param name="Id">The plug-in's id, its assembly name.</param>
/// <param name="Name">The display name from the plug-in's manifest, or the id when the plug-in needs reinstall.</param>
/// <param name="Version">The plug-in assembly's version, or empty when the plug-in needs reinstall.</param>
/// <param name="HasNewerVersion">Whether the last <c>plugins.newestCompatible</c> check for this plug-in found a newer version it can run.</param>
/// <param name="NeedsReinstall">Whether the assembly targets a plug-in SDK the engine can't run. The plug-in stays listed but doesn't load until a supported version replaces it.</param>
public sealed record InstalledPlugIn(string Id, string Name, string Version, bool Enabled, bool HasNewerVersion, bool NeedsReinstall);

/// <summary>
/// Fires each time a plug-in hook runs, whether through <c>plugins.run</c> or in response to a change in the save.
/// </summary>
/// <param name="PlugInId">The plug-in's id, its assembly name.</param>
/// <param name="HookId">The hook's id, the full name of its .NET type.</param>
/// <param name="Outcome">What the hook asks the host to do, or <c>null</c> when it failed.</param>
/// <param name="Failure">Why the hook failed, or <c>null</c> when it succeeded.</param>
public sealed record PlugInRan(string PlugInId, string HookId, PlugInOutcome? Outcome, HookFailure? Failure) : IEngineEvent;

/// <summary>
/// The error a plug-in hook threw.
/// </summary>
/// <param name="Type">The .NET exception's type name, such as <c>InvalidOperationException</c>.</param>
public sealed record HookFailure(string Type, string Message);

/// <summary>
/// A hook failure the engine kept, as listed by <c>plugins.failures</c>. The engine keeps the 20 most recent.
/// </summary>
/// <param name="Id">Pass it to <c>plugins.dismissFailure</c> to remove the failure.</param>
/// <param name="PlugInId">The plug-in's id, its assembly name.</param>
/// <param name="HookId">The hook's id, the full name of its .NET type.</param>
/// <param name="StackTrace">The .NET stack trace, when there is one.</param>
public sealed record PlugInFailure(int Id, string PlugInId, string HookId, string Message, string? StackTrace);
