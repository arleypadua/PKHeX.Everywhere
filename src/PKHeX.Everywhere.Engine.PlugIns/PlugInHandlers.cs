using PKHeX.Everywhere.Engine.Dtos;

namespace PKHeX.Everywhere.Engine.PlugIns;

public static class PlugInHandlers
{
    [Query("plugins.failures", Topics.All)]
    public static PlugInFailure[] Failures(Session session) => PlugInHost.Of(session).Failures.ToArray();

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
        if (ran.Failure is { } failure) throw new EngineException(ErrorCodes.PlugInFailed, failure.Message, failure);

        slot?.Save();
        return PlugInOutcome.From(ran.Outcome!);
    }

    [Query("plugins.pages", Topics.All)]
    public static DeclaredPage[] Pages(Session session) => PlugInHost.Of(session).Pages().ToArray();

    [Query("plugins.pageModule", Topics.All)]
    public static string PageModule(Session session, string plugInId, string path) =>
        PlugInHost.Of(session).PageModule(plugInId, path);

    [Query("plugins.installed", Topics.PlugIns)]
    public static InstalledPlugIn[] Installed(Session session) => PlugInHost.Of(session).Installed().ToArray();

    [Query("plugins.state", Topics.PlugIns)]
    public static PlugInState State(Session session, string id)
    {
        var host = PlugInHost.Of(session);
        if (host.Find(id) is null) throw new EngineException(ErrorCodes.NotFound, $"No plug-in {id}.");

        return PlugInState.From(host.State(id));
    }

    [Query("plugins.isSupported")]
    public static bool IsSupported(byte[] assembly) => PlugInHost.IsSupported(assembly);

    [Command("plugins.newestCompatible", Topics.PlugIns)]
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
}
