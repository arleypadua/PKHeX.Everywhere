namespace PKHeX.Everywhere.Engine;

/// <summary>
/// A domain change a command announces. Holds only primitives or Engine DTOs, and stays in-process.
/// </summary>
public interface IEngineEvent;

public sealed record ItemChanged(int ItemId, int Count) : IEngineEvent;
