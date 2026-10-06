using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

[EntityHook("useLoadedGame")]
public static class GameHandlers
{
    private const string DefaultFileName = "save.sav";

    [Query("game.get", Topics.Game)]
    public static SaveSummary? Get(Session session) => session.Game?.ToSummary(session.FileName);

    [Query("game.version", Topics.Game)]
    public static SaveVersion? Version(Session session) => session.Game?.ToVersion();

    /// <summary>
    /// Reads the save's play time, gym badges and Pokédex counts.
    /// </summary>
    [Query("game.progress", Topics.Game, Topics.Events, Topics.Party, Topics.Box)]
    public static GameProgress Progress(Game game) => game.Progress.ToDto();

    // Serialising compacts LGPE storage, which moves box slots.
    [Command("game.file", Topics.Party, Topics.Box)]
    public static LoadedSave? File(Session session) => session.Game?.ToFile(session.FileName);

    /// <summary>
    /// Loads a save file, replacing the one loaded before.
    /// </summary>
    /// <param name="data">The save file's bytes.</param>
    /// <param name="fileName">The file name to export the save with. Defaults to the name of a <c>File</c>, or <c>save.sav</c>.</param>
    /// <param name="formatId">
    /// The id of a save format from <c>game.formats()</c> to load the save with, skipping detection. Pass <c>pkhex</c> to load it with PKHeX's own detection.
    /// An unknown id fails with <c>not-found</c>, and a save the format can't read fails with <c>invalid-save</c>.
    /// Without it, a save that might be in one of the formats fails with <c>format-choice-required</c>, whose <c>candidates</c> list them.
    /// </param>
    [Command("game.load", Topics.All)]
    public static void Load(Session session, byte[] data, string? fileName = null, string? formatId = null)
    {
        fileName ??= DefaultFileName;
        session.Load(LoadGame(data, fileName, formatId), fileName);
    }

    internal static Game LoadGame(byte[] data, string fileName, string? formatId)
    {
        var format = formatId is null
            ? null
            : SaveFormats.Find(formatId) ?? throw new EngineException(ErrorCodes.NotFound, $"There is no save format '{formatId}'.");
        try
        {
            return Game.LoadFrom(data, fileName, format);
        }
        catch (GameNotLoadedException e)
        {
            throw new EngineException(ErrorCodes.InvalidSave, $"'{fileName}' is not a supported save file.", e);
        }
        catch (FormatChoiceRequiredException e)
        {
            throw new EngineException(ErrorCodes.FormatChoiceRequired, e.Message, e)
            {
                Candidates = e.Candidates.Select(candidate => candidate.ToEntry()).ToArray(),
            };
        }
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

    [Query("game.moves", Topics.Game)]
    public static Choice[] Moves(Game game) => game.Options.Moves.ToChoices();

    /// <summary>
    /// Names the type ids in a Pokémon's <c>types</c>, from Normal (0) to Fairy (17). The list is the same for every save.
    /// </summary>
    [Query("game.types", Topics.Game)]
    public static Choice[] Types(Game game) => game.Options.Types.ToChoices();

    [Query("game.blankVersions")]
    public static VersionEntry[] BlankVersions() =>
        GameVersionRepository.Instance.Blank.Select(version => version.ToEntry()).ToArray();

    /// <summary>
    /// Lists the save formats PKHeX doesn't know, such as ROM hacks, that <c>game.load()</c> can load a save with.
    /// </summary>
    [Query("game.formats")]
    public static FormatEntry[] Formats() => SaveFormats.All.Select(format => format.ToEntry()).ToArray();

    /// <summary>
    /// Turns on a save format that's registered off, such as a ROM hack that hasn't been validated yet.
    /// Until then, <c>game.formats()</c> leaves it out and a save it recognizes fails with <c>invalid-save</c>.
    /// </summary>
    /// <param name="id">The format's id. An unknown id fails with <c>not-found</c>.</param>
    [Command("game.enableFormat")]
    public static void EnableFormat(string id)
    {
        if (!SaveFormats.Enable(id)) throw new EngineException(ErrorCodes.NotFound, $"There is no save format '{id}'.");
    }

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
