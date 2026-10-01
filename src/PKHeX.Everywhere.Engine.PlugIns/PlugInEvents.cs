namespace PKHeX.Everywhere.Engine.PlugIns;

public sealed record PlugInInstalled(string PlugInId, string Version) : IEngineEvent;

public sealed record PlugInUpdated(string PlugInId, string Version) : IEngineEvent;
