using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.PlugIns;

public sealed record DeclaredPage(string PlugInId, string Path, string? Title, PageLayout Layout);
