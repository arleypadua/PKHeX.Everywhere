using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class TeamHandlers
{
    /// <summary>
    /// Lists every registered team in order, empty ones included. Only Stadium saves have teams, so any other save returns none.
    /// </summary>
    [Query("team.list", Topics.Team)]
    public static TeamEntry[] List(Game game) => game.Teams?.All.Select(team => team.ToEntry()).ToArray() ?? [];

    /// <summary>
    /// The team's Pokémon in slot order, with handles of source <c>team</c>.
    /// Fails with <c>out-of-range</c> for an unknown team, and <c>not-supported</c> for a save without teams.
    /// </summary>
    [Query("team.get", Topics.Team)]
    public static PokemonSummary[] Get(Game game, int team) => InRange(() => game.RequireTeams().Get(team)).Members
        .Select((pokemon, slot) => pokemon.ToSummary(PokemonHandle.InTeam(team, slot)))
        .ToArray();

    /// <summary>
    /// Copies a Pokémon into a team slot, leaving it where it is, as Stadium registers a team. A filled slot is replaced.
    /// A slot past the filled ones puts the Pokémon in the first empty slot, so a team never has a gap.
    /// Fails with <c>not-supported</c> for a save without teams, <c>out-of-range</c> for an unknown team or slot,
    /// <c>not-found</c> when <c>from</c> has no Pokémon, and <c>draft-not-allowed</c> for the draft.
    /// </summary>
    /// <param name="from">The Pokémon to copy, usually in a box.</param>
    /// <param name="to">The team slot to copy it to.</param>
    /// <returns>The copy, with the handle of the slot it landed in.</returns>
    [Command("team.place")]
    public static PokemonSummary Place(Game game, PokemonHandle from, TeamSlot to)
    {
        var teams = game.RequireTeams();
        var pokemon = game.FindSaved(from).Pokemon;
        var slot = InRange(() => teams.Place(to.Team, to.Slot, pokemon));
        var at = PokemonHandle.InTeam(to.Team, slot);
        return teams.Get(to.Team).Members[slot].ToSummary(at);
    }

    /// <summary>
    /// Empties a team slot. The Pokémon after it move up, so a team never has a gap. Clearing an empty slot does nothing.
    /// A draft opened from the cleared slot or one after it is dropped.
    /// Fails with <c>not-supported</c> for a save without teams and <c>out-of-range</c> for an unknown team or slot.
    /// </summary>
    [Command("team.clear")]
    public static void Clear(Session session, Game game, TeamSlot at)
    {
        var teams = game.RequireTeams();
        InRange(() => teams.Clear(at.Team, at.Slot));

        if (session.Draft?.From is { Source: SlotSource.Team } from && from.Team == at.Team && from.Slot >= at.Slot)
        {
            session.Draft = null;
            session.AlsoWrote(Topics.Draft);
        }
    }

    private static void InRange(Action change) => InRange(() =>
    {
        change();
        return 0;
    });

    private static T InRange<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new EngineException(ErrorCodes.OutOfRange, e.Message, e);
        }
    }
}
