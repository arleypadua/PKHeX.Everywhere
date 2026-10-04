using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class ImperiumSaveTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Imperium);

    [Fact]
    public void LoadingWhileTheFormatIsOffFailsWithInvalidSave()
    {
        var session = new Session();

        Error(Dispatch(session, "game.load", Args(Convert.ToBase64String(Fixture), "imperium.sav", null!))).Should().Be("invalid-save");

        session.Game.Should().BeNull();
    }

    [Fact]
    public void TheFormatIsNotListedWhileItIsOff() =>
        Value(Dispatch(new Session(), "game.formats", "[]"))!.AsArray()
            .Select(format => format!["id"]!.GetValue<string>()).Should().NotContain("emerald-imperium");
}
