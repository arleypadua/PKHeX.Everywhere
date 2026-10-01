using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

[EntityHook("useLoadedGame")]
public static class GameHandlers
{
    [Query("game.get", Topics.Game)]
    public static SaveSummary? Get(Session session) => session.Game?.ToSummary(session.FileName);

    [Command("game.load", Topics.All)]
    public static void Load(Session session, byte[] data, string fileName)
    {
        Game game;
        try
        {
            game = Game.LoadFrom(data, fileName);
        }
        catch (GameNotLoadedException e)
        {
            throw new EngineException(ErrorCodes.InvalidSave, $"'{fileName}' is not a supported save file.", e);
        }

        session.Load(game, fileName);
    }

    [Command("game.close", Topics.All)]
    public static void Close(Session session) => session.Close();
}
