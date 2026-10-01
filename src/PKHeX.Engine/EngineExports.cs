using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using PKHeX.Facade;

namespace PKHeX.Engine;

[SupportedOSPlatform("browser")]
public static partial class EngineExports
{
    [JSExport]
    public static string LoadSave(byte[] bytes, string fileName) => Run(EngineJson.Default.ResultSaveSummaryDto, () =>
    {
        Session.Load(Game.LoadFrom(bytes, fileName), fileName);
        return Summary();
    });

    [JSExport]
    public static string GetSave() => Run(EngineJson.Default.ResultSaveSummaryDto, Summary);

    [JSExport]
    public static string GetParty() => Run(EngineJson.Default.ResultPartySlotDtoArray, () =>
        LoadedGame().Trainer.Party.Pokemons
            .Select((p, i) => new PartySlotDto(i, p.Species.Id, p.Species.Name, p.Nickname, p.Level, p.IsShiny, p.HeldItem.Name))
            .ToArray());

    [JSImport("globalThis.pkhexEngine.onChanged")]
    private static partial void NotifyChanged(string topic);

    [JSImport("globalThis.pkhexEngine.onReady")]
    public static partial void NotifyReady();

    public static void WireEvents() => Session.Changed += () => NotifyChanged("save");

    private static SaveSummaryDto Summary()
    {
        var game = LoadedGame();
        return new SaveSummaryDto(Session.FileName ?? "", game.SaveVersion.Name, game.Trainer.Name);
    }

    private static Game LoadedGame() =>
        Session.Game ?? throw new EngineException("no-save", "No save is loaded.");

    private static string Run<T>(JsonTypeInfo<Result<T>> type, Func<T> query)
    {
        Result<T> result;
        try
        {
            result = Result<T>.Success(query());
        }
        catch (EngineException e)
        {
            result = Result<T>.Failure(e.Code, e.Message);
        }
        catch (Exception e)
        {
            result = Result<T>.Failure("unexpected", e.Message);
        }

        return JsonSerializer.Serialize(result, type);
    }
}

public class EngineException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
