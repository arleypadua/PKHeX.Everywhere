using System.Collections.Frozen;
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

    /// <summary>
    /// The choice that loads a save with PKHeX's own detection, for a save a registered format only might be.
    /// </summary>
    public static ISaveFormat PKHeX { get; } = new PKHeXFormat();

    public static void Register(ISaveFormat format) => ImmutableInterlocked.Update(
        ref _all,
        all => all.Any(registered => registered.Id == format.Id) ? all : all.Add(format));

    public static ISaveFormat? Find(string id) =>
        id == PKHeX.Id ? PKHeX : _all.FirstOrDefault(format => format.Id == id);

    internal static Game? Detect(byte[] data)
    {
        var matches = _all.Select(format => (Format: format, Match: format.Detect(data))).ToArray();
        if (matches.FirstOrDefault(match => match.Match == SaveFormatMatch.Certain).Format is { } certain)
            return new Game(certain.Load(data), certain);

        var possible = matches.Where(match => match.Match == SaveFormatMatch.Possible).Select(match => match.Format).ToArray();
        return possible.Length == 0 ? null : throw new FormatChoiceRequiredException(possible);
    }

    internal sealed class PKHeXFormat : ISaveFormat
    {
        public string Id => "pkhex";
        public string Name => "PKHeX";
        public GameVersion BaseGame => GameVersion.Any;
        public IReadOnlySet<Capability> Capabilities { get; } = Enum.GetValues<Capability>().ToFrozenSet();
        public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => SaveFormatMatch.No;
        public SaveFile Load(byte[] data) => SaveUtil.GetSaveFile(data) ?? throw new GameNotLoadedException();
    }
}

public class FormatChoiceRequiredException(IReadOnlyList<ISaveFormat> candidates)
    : Exception($"The save might be {string.Join(" or ", candidates.Select(format => format.Name))}. Choose a format to load it with.")
{
    public IReadOnlyList<ISaveFormat> Candidates { get; } = candidates;
}
