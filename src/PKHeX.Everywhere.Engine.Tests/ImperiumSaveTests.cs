using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class ImperiumSaveTests
{
    public static TheoryData<string, byte[]> Saves => new()
    {
        { "as found", File.ReadAllBytes(SaveFilePath.Imperium) },
        { "readable as Emerald", SaveFilePath.ImperiumReadableAsEmerald() },
    };

    [Theory]
    [MemberData(nameof(Saves))]
    public void LoadingWhileTheFormatIsOffFailsWithInvalidSave(string _, byte[] save)
    {
        var session = new Session();

        Error(Dispatch(session, "game.load", Args(Convert.ToBase64String(save), "imperium.sav", null!))).Should().Be("invalid-save");

        session.Game.Should().BeNull();
    }

    [Fact]
    public void TheFormatIsNotListedWhileItIsOff() =>
        Value(Dispatch(new Session(), "game.formats", "[]"))!.AsArray()
            .Select(format => format!["id"]!.GetValue<string>()).Should().NotContain("emerald-imperium");
}
