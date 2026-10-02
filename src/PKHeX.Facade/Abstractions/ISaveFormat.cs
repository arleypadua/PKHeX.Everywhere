using PKHeX.Core;

namespace PKHeX.Facade.Abstractions;

public interface ISaveFormat
{
    string Id { get; }
    string Name { get; }
    GameVersion BaseGame { get; }

    SaveFormatMatch Detect(ReadOnlySpan<byte> data);
    SaveFile Load(byte[] data);
}

public enum SaveFormatMatch
{
    No,
    Possible,
    Certain,
}
