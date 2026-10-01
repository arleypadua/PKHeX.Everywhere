using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace PKHeX.Everywhere.Engine;

[SupportedOSPlatform("browser")]
public static partial class EngineExports
{
    [JSExport]
    public static string Call(string name, string args) => Dispatcher.Dispatch(Session.Current, name, args);

    // Blazor.start() resolves before the renderer attaches, so the host signals readiness from its first render.
    // The flag covers JS that starts waiting after the signal, the callback covers JS that is already waiting.
    public static void SignalReady()
    {
        JSHost.GlobalThis.SetProperty("pkhexEngineReady", true);
        if (JSHost.GlobalThis.GetTypeOfProperty("pkhexEngineOnReady") == "function") OnReady();
    }

    [JSImport("globalThis.pkhexEngineOnReady")]
    private static partial void OnReady();
}
