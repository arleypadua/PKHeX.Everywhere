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
}

public enum SaveFormatMatch
{
    No,
    Possible,
    Certain,
}

public record SaveFormatDescription(string Id, string Name, GameVersion BaseGame);
