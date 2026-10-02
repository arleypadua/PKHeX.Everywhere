namespace PKHeX.Facade.Abstractions;

public interface IMoveList
{
    IReadOnlySet<ushort> Moves { get; }
}
