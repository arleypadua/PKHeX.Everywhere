using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class HeartAndSoulSaveTests
{
    [Fact]
    public void LoadingWhileTheFormatIsOffFailsWithInvalidSave()
    {
        var session = new Session();

        Error(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.HeartAndSoul)), "heartandsoul.sav", null!)))
            .Should().Be("invalid-save");

        session.Game.Should().BeNull();
    }

    [Fact]
    public void TheFormatIsNotListedWhileItIsOff() =>
        Value(Dispatch(new Session(), "game.formats", "[]"))!.AsArray()
            .Select(format => format!["id"]!.GetValue<string>()).Should().NotContain("heart-and-soul");
}
