using PKHeX.Core;

namespace PKHeX.Facade.Abstractions;

public interface ISaveFormat
{
    string Id { get; }
    string Name { get; }
    GameVersion BaseGame { get; }
    IReadOnlySet<Capability> Capabilities { get; }

    IGameDataSource GameData(SaveFile save) => new PKHeXGameData(save);

    SaveFormatMatch Detect(ReadOnlySpan<byte> data);
    SaveFile Load(byte[] data);

    /// <summary>
    /// An empty save in the format, to read its Pokémon and name its species and items without a loaded save. Null when the format can't make one.
    /// </summary>
    SaveFile? Blank() => null;
}

public enum SaveFormatMatch
{
    No,
    Possible,
    Certain,
}

public record SaveFormatDescription(string Id, string Name, GameVersion BaseGame);
