using PKHeX.Everywhere.Engine.Dtos;

namespace PKHeX.Everywhere.Engine.PlugIns;

public static class PlugInHandlers
{
    [Query("plugins.failures", Topics.All)]
    public static PlugInFailure[] Failures(Session session) => PlugInHost.Of(session).Failures.ToArray();

    [Command("plugins.dismissFailure", Topics.PlugIns)]
    public static void DismissFailure(Session session, int id) => PlugInHost.Of(session).Dismiss(id);

    [Query("plugins.actions", Topics.All)]
    public static PlugInAction[] Actions(Session session, ActionPlacement placement, PokemonHandle? target)
    {
        if (placement != ActionPlacement.Quick)
        {
            if (target is null) throw new EngineException(ErrorCodes.BadArguments, $"{placement} actions need a target Pokémon.");
            session.Find(target);
        }

        return PlugInHost.Of(session).Actions(placement).ToArray();
    }

    [Command("plugins.run", Topics.All)]
    public static async Task<PlugInOutcome> Run(Session session, string id, PokemonHandle? target)
    {
        var slot = target is null ? null : session.Find(target);
        var ran = await PlugInHost.Of(session).RunAction(id, slot?.Pokemon);
        if (ran.Failure is { } failure) throw new EngineException(ErrorCodes.PlugInFailed, failure.Message);

        slot?.Save();
        return ran.Outcome!;
    }

    [Query("plugins.pages", Topics.All)]
    public static DeclaredPage[] Pages(Session session) => PlugInHost.Of(session).Pages().ToArray();

    [Query("plugins.pageModule", Topics.All)]
    public static string PageModule(Session session, string plugInId, string path) =>
        PlugInHost.Of(session).PageModule(plugInId, path);

    [Query("plugins.setting", Topics.PlugIns)]
    public static PlugInSetting? Setting(Session session, string plugInId, string key)
    {
        var plugIn = PlugInHost.Of(session).Find(plugInId) ?? throw new EngineException(ErrorCodes.NotFound, $"No plug-in {plugInId}.");
        return plugIn.Settings.GetOrDefault(key) is { } value ? PlugInSetting.From(key, value) : null;
    }

    [Query("plugins.installed", Topics.PlugIns)]
    public static InstalledPlugIn[] Installed(Session session) => PlugInHost.Of(session).Installed().ToArray();

    [Query("plugins.state", Topics.PlugIns)]
    public static PlugInState State(Session session, string id) => PlugInState.From(HostOf(session, id).State(id));

    [Query("plugins.details", Topics.PlugIns)]
    public static PlugInDetails Details(Session session, string id)
    {
        var host = HostOf(session, id);
        return PlugInDetails.From(host.Find(id)!, host.HasNewerVersion(id));
    }

    [Command("plugins.setEnabled", Topics.PlugIns)]
    public static PlugInState SetEnabled(Session session, string id, bool enabled) =>
        Write(session, id, host => host.SetEnabled(id, enabled));

    [Command("plugins.setHookEnabled", Topics.PlugIns)]
    public static PlugInState SetHookEnabled(Session session, string id, string hookId, bool enabled) =>
        Write(session, id, host => host.SetToggle(id, hookId, enabled));

    [Command("plugins.updateSetting", Topics.PlugIns)]
    public static PlugInState UpdateSetting(Session session, string id, PlugInSetting setting) =>
        Write(session, id, host => host.UpdateSetting(
            id,
            setting.Key,
            setting.ToValue() ?? throw new EngineException(ErrorCodes.BadArguments, $"Setting {setting.Key} has no value.")));

    [Query("plugins.isSupported")]
    public static bool IsSupported(byte[] assembly) => PlugInHost.IsSupported(assembly);

    [Command("plugins.newestCompatible")]
    public static PublishedVersion? NewestCompatible(Session session, PublishedVersion[] versions, string? plugInId) =>
        PlugInHost.Of(session).NewestCompatible(plugInId, versions);

    [Command("plugins.register", Topics.PlugIns)]
    public static InstalledPlugIn Register(Session session, byte[] assembly, PlugInState? stored)
    {
        try
        {
            return PlugInHost.Of(session).Install(assembly, stored?.ToStored());
        }
        catch (IncompatiblePlugInException e)
        {
            throw new EngineException(ErrorCodes.BadArguments, e.Message);
        }
    }

    [Command("plugins.unregister", Topics.PlugIns)]
    public static void Unregister(Session session, string id) => PlugInHost.Of(session).Unregister(id);

    private static PlugInHost HostOf(Session session, string id)
    {
        var host = PlugInHost.Of(session);
        return host.Find(id) is null ? throw new EngineException(ErrorCodes.NotFound, $"No plug-in {id}.") : host;
    }

    private static PlugInState Write(Session session, string id, Action<PlugInHost> write)
    {
        var host = HostOf(session, id);
        try
        {
            write(host);
        }
        catch (Exception e) when (e is KeyNotFoundException or InvalidOperationException)
        {
            throw new EngineException(ErrorCodes.BadArguments, e.Message);
        }

        return PlugInState.From(host.State(id));
    }
}
