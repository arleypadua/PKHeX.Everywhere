using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

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

    [Command("game.export")]
    public static ExportedSave Export(Session session, Game game)
    {
        var bytes = game.ToByteArray();
        session.Raise(new GameExported());
        return new ExportedSave(bytes, session.FileName ?? string.Empty);
    }

    [Query("game.blankVersions")]
    public static VersionEntry[] BlankVersions() =>
        GameVersionRepository.Instance.Blank.Select(version => version.ToEntry()).ToArray();

    [Command("game.loadBlank", Topics.All)]
    public static void LoadBlank(Session session, int version)
    {
        var definition = GameVersionRepository.Instance.FindBlank(version)
            ?? throw new EngineException(ErrorCodes.NotFound, $"There is no blank save for version {version}.");

        session.Load(Game.EmptyOf(definition), definition.Name);
    }

    [Command("game.close", Topics.All)]
    public static void Close(Session session) => session.Close();
}
