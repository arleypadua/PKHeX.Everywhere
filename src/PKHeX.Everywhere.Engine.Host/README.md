# PKHeX.Everywhere.Engine.Host

The .NET app the web app boots. It has no UI and no Blazor reference. `Program.cs` points PKHeX's AES and MD5 at the `pkhex-crypto` module, attaches the plug-in host to `Session.Current` and signals that the Engine is ready.

`wasmHost` in the Engine SDK loads `_framework/dotnet.js`, registers `pkhex-crypto` and runs `Main`. The browser runtime has no AES or MD5, so `pkhex-crypto` uses crypto-js.

Publishing uses `Microsoft.NET.Sdk.BlazorWebAssembly`'s trim defaults, copied into the csproj. Plug-ins load with `Assembly.Load(bytes)`, so the trimmer never sees them, and a framework API only a plug-in calls can be trimmed away. After changing a trim setting or a reference, publish in Release and run every plug-in.
