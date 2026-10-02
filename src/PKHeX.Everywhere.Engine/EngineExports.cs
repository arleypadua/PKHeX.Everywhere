using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace PKHeX.Everywhere.Engine;

[SupportedOSPlatform("browser")]
public static partial class EngineExports
{
    [JSExport]
    public static Task<string> Call(string name, string args) => Dispatcher.Dispatch(Session.Current, name, args);

    // Blazor.start() resolves before the renderer attaches, so the host signals readiness from its first render.
    // The flag covers JS that starts waiting after the signal, the callback covers JS that is already waiting.
    public static void SignalReady()
    {
        Session.Current.Changed += topics =>
        {
            if (JSHost.GlobalThis.GetTypeOfProperty("pkhexEngineOnChange") == "function") OnChange(topics);
        };
        Session.Current.Published += engineEvent =>
        {
            if (JSHost.GlobalThis.GetTypeOfProperty("pkhexEngineOnEvent") == "function" && Session.Current.Serialize(engineEvent) is { } json)
                OnEvent(json);
        };

        JSHost.GlobalThis.SetProperty("pkhexEngineReady", true);
        if (JSHost.GlobalThis.GetTypeOfProperty("pkhexEngineOnReady") == "function") OnReady();
    }

    [JSImport("globalThis.pkhexEngineOnReady")]
    private static partial void OnReady();

    [JSImport("globalThis.pkhexEngineOnChange")]
    private static partial void OnChange([JSMarshalAs<JSType.Array<JSType.String>>] string[] topics);

    [JSImport("globalThis.pkhexEngineOnEvent")]
    private static partial void OnEvent(string engineEvent);
}
