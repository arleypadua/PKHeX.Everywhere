using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class GameProgressTests
{
    [Theory]
    [InlineData(SaveFilePath.Yellow, """{"playTime":{"hours":76,"minutes":47,"seconds":3},"badges":{"earned":8,"total":8},"pokedex":{"seen":151,"caught":151}}""")]
    [InlineData(SaveFilePath.Crystal, """{"playTime":{"hours":24,"minutes":23,"seconds":8},"badges":{"earned":7,"total":16},"pokedex":{"seen":153,"caught":26}}""")]
    [InlineData(SaveFilePath.Emerald, """{"playTime":{"hours":0,"minutes":37,"seconds":42},"badges":{"earned":0,"total":8},"pokedex":{"seen":386,"caught":386}}""")]
    [InlineData(SaveFilePath.FireRed, """{"playTime":{"hours":500,"minutes":0,"seconds":0},"badges":{"earned":8,"total":8},"pokedex":{"seen":386,"caught":386}}""")]
    [InlineData(SaveFilePath.HgSs, """{"playTime":{"hours":29,"minutes":25,"seconds":29},"badges":{"earned":2,"total":16},"pokedex":{"seen":42,"caught":16}}""")]
    [InlineData(SaveFilePath.LetsGoPikachu, """{"playTime":{"hours":41,"minutes":18,"seconds":1},"badges":null,"pokedex":{"seen":75,"caught":37}}""")]
    public void ReportsTheProgressPKHeXReads(string saveFile, string expected) =>
        Value(Dispatch(Loaded(saveFile), "game.progress", "[]"))!.ToJsonString().Should().Be(expected);

    [Theory]
    [InlineData(SaveFilePath.RadicalRed, """{"playTime":{"hours":47,"minutes":30,"seconds":56},"badges":null,"pokedex":null}""")]
    [InlineData(SaveFilePath.Unbound, """{"playTime":{"hours":999,"minutes":59,"seconds":59},"badges":null,"pokedex":null}""")]
    [InlineData(SaveFilePath.Imperium, """{"playTime":{"hours":189,"minutes":57,"seconds":39},"badges":null,"pokedex":null}""")]
    public void ReportsOnlyThePlayTimeOfARomHack(string saveFile, string expected) =>
        Value(Dispatch(Loaded(saveFile), "game.progress", "[]"))!.ToJsonString().Should().Be(expected);

    [Theory]
    [InlineData(SaveFilePath.RadicalRed)]
    [InlineData(SaveFilePath.Unbound)]
    public void ReadsTheSamePlayTimeAsPKHeXReadingACfruSaveAsFireRed(string saveFile)
    {
        var vanilla = SaveUtil.GetSaveFile(File.ReadAllBytes(saveFile))!;

        Value(Dispatch(Loaded(saveFile), "game.progress", "[]"))!["playTime"]!.ToJsonString()
            .Should().Be($$"""{"hours":{{vanilla.PlayedHours}},"minutes":{{vanilla.PlayedMinutes}},"seconds":{{vanilla.PlayedSeconds}}}""");
    }

    [Fact]
    public void ReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "game.progress", "[]")).Should().Be("no-save");
}
