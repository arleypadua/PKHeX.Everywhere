namespace PKHeX.Facade.Abstractions;

/// <summary>
/// A save that stores only these moves, where PKHeX's saves store every move up to <c>MaxMoveID</c>.
/// </summary>
public interface IMoveList
{
    IReadOnlySet<ushort> Moves { get; }
}
