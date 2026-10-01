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
}
