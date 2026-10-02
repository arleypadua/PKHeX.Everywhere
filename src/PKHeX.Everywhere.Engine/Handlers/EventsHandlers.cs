using System.Collections.Immutable;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Events;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class EventsHandlers
{
    [Query("events.get", Topics.Events, Topics.Inventory)]
    public static SaveEvents? Get(Game game)
    {
        game.Require(Capability.Events);
        return game.Events?.ToSaveEvents();
    }

    [Query("events.flag", Topics.Events)]
    public static bool Flag(Game game, int index) => InRange(() => Require(game).GetFlag(index));

    [Command("events.setFlag", Topics.Events)]
    public static void SetFlag(Game game, int index, bool value) => InRange(() => Require(game).SetFlag(index, value));

    [Command("events.setWork", Topics.Events)]
    public static void SetWork(Game game, int index, int value) => InRange(() => Require(game).SetWork(index, value));

    [Command("events.giveTickets", Topics.Inventory)]
    public static string[] GiveTickets(Session session, Game game, bool includeOldSeaMap)
    {
        game.Require(Capability.Events);
        var tickets = game.Events?.Gen3?.Tickets
            ?? throw new EngineException(ErrorCodes.NotFound, "Only Generation 3 saves have event tickets.");

        ImmutableList<ItemDefinition> added;
        try
        {
            added = tickets.Give(includeOldSeaMap);
        }
        catch (InvalidOperationException e)
        {
            throw new EngineException(ErrorCodes.PouchFull, e.Message, e);
        }

        foreach (var ticket in added) session.Raise(new ItemChanged(ticket.Id, 1));
        return added.Select(t => t.Name).ToArray();
    }

    private static GameEvents Require(Game game)
    {
        game.Require(Capability.Events);
        return game.Events ?? throw new EngineException(ErrorCodes.NotFound, "This save has no events.");
    }

    private static void InRange(Action action) => InRange(() =>
    {
        action();
        return true;
    });

    private static T InRange<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new EngineException(ErrorCodes.OutOfRange, e.Message, e);
        }
    }
}
