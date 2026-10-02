using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

[EntityHook("useLoadedGame")]
public static class GameHandlers
{
    [Query("game.get", Topics.Game)]
    public static SaveSummary? Get(Session session) => session.Game?.ToSummary(session.FileName);

    [Query("game.version", Topics.Game)]
    public static SaveVersion? Version(Session session) => session.Game?.ToVersion();

    // Serialising commits pending edits and can rewrite slots, as LGPE does when its storage is compacted.
    [Command("game.file", Topics.All)]
    public static LoadedSave? File(Session session) => session.Game?.ToFile(session.FileName);

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

    [Command("game.export", Topics.Party, Topics.Box)]
    public static ExportedSave Export(Session session, Game game)
    {
        var bytes = game.ToByteArray();
        session.Raise(new GameExported(game.ToOverview()));
        return new ExportedSave(bytes, session.FileName ?? string.Empty);
    }

    [Query("game.natures", Topics.Game)]
    public static Choice[] Natures(Game game) => game.Options.Natures.ToChoices();

    [Query("game.balls", Topics.Game)]
    public static Choice[] Balls(Game game) => game.Options.Balls.ToChoices();

    [Query("game.languages", Topics.Game)]
    public static Choice[] Languages(Game game) => game.Options.Languages.ToChoices();

    [Query("game.heldItems", Topics.Game)]
    public static Choice[] HeldItems(Game game) => game.Options.HeldItems.ToChoices();

    [Query("game.originGames", Topics.Game)]
    public static Choice[] OriginGames(Game game) => game.Options.OriginGames.ToChoices();

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
