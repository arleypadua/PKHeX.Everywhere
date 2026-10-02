using System.Runtime.Versioning;
using PKHeX.Core;
using PKHeX.Everywhere.Engine;
using PKHeX.Everywhere.Engine.Host;
using PKHeX.Everywhere.Engine.PlugIns;

[assembly: SupportedOSPlatform("browser")]

RuntimeCryptographyProvider.Aes = new JsAesProvider();
RuntimeCryptographyProvider.Md5 = new JsMd5Provider();

_ = new PlugInHost(Session.Current);

EngineExports.ForwardSessionToJs();
