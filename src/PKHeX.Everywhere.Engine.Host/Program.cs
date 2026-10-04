using System.Runtime.Versioning;
using PKHeX.Core;
using PKHeX.Everywhere.Engine;
using PKHeX.Everywhere.Engine.Host;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Everywhere.RomHacks.Expansion.Imperium;
using PKHeX.Facade;

[assembly: SupportedOSPlatform("browser")]

RuntimeCryptographyProvider.Aes = new JsAesProvider();
RuntimeCryptographyProvider.Md5 = new JsMd5Provider();

SaveFormats.Register(new UnboundFormat());
SaveFormats.Register(new RadicalRedFormat());
SaveFormats.Register(new ImperiumFormat());

_ = new PlugInHost(Session.Current);

EngineExports.ForwardSessionToJs();
