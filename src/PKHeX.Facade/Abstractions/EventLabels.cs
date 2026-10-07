namespace PKHeX.Facade.Abstractions;

/// <summary>
/// What a save's event flags and work values are called. A Save format supplies these when the hack renumbers
/// or renames what its base game stores, so the editor labels what the hack means.
/// </summary>
/// <param name="Flags">Names by flag index.</param>
/// <param name="Work">Names by work index.</param>
public sealed record EventLabels(IReadOnlyList<EventLabel> Flags, IReadOnlyList<EventLabel> Work);

/// <param name="Index">The index the save stores it at.</param>
/// <param name="Name">What to show.</param>
/// <param name="Category">Groups related entries in the editor.</param>
public readonly record struct EventLabel(int Index, string Name, string Category);
