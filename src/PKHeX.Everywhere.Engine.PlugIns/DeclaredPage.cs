using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.PlugIns;

/// <summary>
/// A page an enabled plug-in declares, as listed by <c>plugins.pages</c>.
/// </summary>
/// <param name="PlugInId">The plug-in's id, its assembly name.</param>
/// <param name="Path">The page's path within the plug-in. Pass it with <c>plugInId</c> to <c>plugins.pageModule</c> to get the page's JavaScript module.</param>
/// <param name="Title">The page's title, or <c>null</c> when the plug-in gives none.</param>
public sealed record DeclaredPage(string PlugInId, string Path, string? Title, PageLayout Layout);
