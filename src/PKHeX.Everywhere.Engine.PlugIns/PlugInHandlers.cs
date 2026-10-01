namespace PKHeX.Everywhere.Engine.PlugIns;

public static class PlugInHandlers
{
    [Query("plugins.failures", Topics.All)]
    public static PlugInFailure[] Failures(Session session) => PlugInHost.Of(session).Failures.ToArray();
}
