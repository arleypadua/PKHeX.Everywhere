namespace PKHeX.Everywhere.Engine.PlugIns;

/// <summary>
/// Fires when <c>plugins.register</c> loads a plug-in the engine didn't have, without a saved state.
/// </summary>
/// <param name="PlugInId">The plug-in's id, its assembly name.</param>
/// <param name="Version">The plug-in assembly's version.</param>
public sealed record PlugInInstalled(string PlugInId, string Version) : IEngineEvent;

/// <summary>
/// Fires when <c>plugins.register</c> replaces a plug-in the engine already had, including one that needed reinstall.
/// </summary>
/// <param name="PlugInId">The plug-in's id, its assembly name.</param>
/// <param name="Version">The newly registered assembly's version.</param>
public sealed record PlugInUpdated(string PlugInId, string Version) : IEngineEvent;
