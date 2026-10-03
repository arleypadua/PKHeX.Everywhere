using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Everywhere.RomHacks.Cfru;

// PKHeX's legality, encounter and Showdown data describe vanilla games, so a CFRU save supports none of it.
public abstract class CfruFormat : ISaveFormat
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public GameVersion BaseGame => GameVersion.FR;
    public IReadOnlySet<Capability> Capabilities => FrozenSet<Capability>.Empty;

    protected abstract bool FireRedMetLocations { get; }

    public IGameDataSource GameData(SaveFile save) => new CfruGameData((CfruSave)save, FireRedMetLocations);

    public abstract SaveFormatMatch Detect(ReadOnlySpan<byte> data);
    public abstract SaveFile Load(byte[] data);
}
