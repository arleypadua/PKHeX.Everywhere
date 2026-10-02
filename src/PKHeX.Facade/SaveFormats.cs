using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Facade;

/// <summary>
/// Save formats PKHeX doesn't know. The host registers them at startup, and loading asks them before PKHeX's own detection.
/// </summary>
public static class SaveFormats
{
    private static ImmutableList<ISaveFormat> _all = [];

    public static IReadOnlyList<ISaveFormat> All => _all;

    public static void Register(ISaveFormat format) => ImmutableInterlocked.Update(
        ref _all,
        all => all.Any(registered => registered.Id == format.Id) ? all : all.Add(format));

    internal static Game? LoadCertain(byte[] data) =>
        _all.FirstOrDefault(format => format.Detect(data) == SaveFormatMatch.Certain) is { } format
            ? new Game(format.Load(data), format)
            : null;
}
