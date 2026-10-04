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
    private static ImmutableList<Registration> _registered = [];

    public static IReadOnlyList<ISaveFormat> All =>
        _registered.Where(registration => registration.Enabled).Select(registration => registration.Format).ToArray();

    /// <summary>
    /// The choice that loads a save with PKHeX's own detection, for a save a registered format only might be.
    /// </summary>
    public static ISaveFormat PKHeX { get; } = new PKHeXFormat();

    public static void Register(ISaveFormat format) => Register(format, enabled: true);

    /// <summary>
    /// Registers a format, or does nothing if one with the same id is registered.
    /// A disabled format is left out of <see cref="All"/> and <see cref="Find"/>, and a save it matches fails to load until it's enabled.
    /// </summary>
    public static void Register(ISaveFormat format, bool enabled) => ImmutableInterlocked.Update(
        ref _registered,
        registered => registered.Any(registration => registration.Format.Id == format.Id)
            ? registered
            : registered.Add(new Registration(format, enabled)));

    /// <summary>
    /// Enables a format registered disabled.
    /// </summary>
    /// <returns><c>false</c> if no format with the id is registered.</returns>
    public static bool Enable(string id)
    {
        var found = false;
        ImmutableInterlocked.Update(ref _registered, registered =>
        {
            var index = registered.FindIndex(registration => registration.Format.Id == id);
            found = index >= 0;
            return found ? registered.SetItem(index, registered[index] with { Enabled = true }) : registered;
        });
        return found;
    }

    public static ISaveFormat? Find(string id) =>
        id == PKHeX.Id ? PKHeX : All.FirstOrDefault(format => format.Id == id);

    internal static Game? Detect(byte[] data)
    {
        var matches = _registered
            .Select(registration => (registration.Format, registration.Enabled, Match: registration.Format.Detect(data)))
            .Where(match => match.Match != SaveFormatMatch.No)
            .OrderByDescending(match => match.Enabled)
            .ToArray();

        if (matches.FirstOrDefault(match => match.Match == SaveFormatMatch.Certain) is { Format: { } certain, Enabled: var enabled })
            return enabled ? new Game(certain.Load(data), certain) : throw new GameNotLoadedException();

        var possible = matches.Where(match => match.Enabled).Select(match => match.Format).ToArray();
        if (possible.Length > 0) throw new FormatChoiceRequiredException(possible);
        return matches.Length == 0 ? null : throw new GameNotLoadedException();
    }

    private sealed record Registration(ISaveFormat Format, bool Enabled);

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
