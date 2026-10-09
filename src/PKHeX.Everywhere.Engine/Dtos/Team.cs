using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// A registered team in the save, as <c>team.list</c> lists it. Empty teams are included. Only Stadium saves have teams.
/// </summary>
/// <param name="Team">Zero-based team number, the same number a team <see cref="PokemonHandle"/> carries.</param>
/// <param name="Name">The name of the trainer who registered the team, as the save stores it, or null when it stores none.</param>
/// <param name="Cup">The cup the team is registered for, such as <c>Poké Cup</c>. Null in Pocket Monsters Stadium, which doesn't sort teams by cup.</param>
/// <param name="Slots">How many slots the team has.</param>
/// <param name="Filled">How many slots hold a Pokémon. A team has no gaps, so these are the first slots.</param>
public record TeamEntry(int Team, string? Name, string? Cup, int Slots, int Filled);

/// <summary>
/// A slot in a registered team.
/// </summary>
/// <param name="Team">Zero-based team number.</param>
/// <param name="Slot">Zero-based slot in the team.</param>
public record TeamSlot(int Team, int Slot) : IHandle
{
    public string Topic() => TopicOf(Team);

    internal static string TopicOf(int? team) => team is { } number ? $"{Topics.Team}/{number}" : Topics.Team;
}

public static class TeamMapping
{
    public static TeamEntry ToEntry(this Team team) => new(team.Number, team.Name, team.Cup, team.Slots, team.Members.Count);
}
