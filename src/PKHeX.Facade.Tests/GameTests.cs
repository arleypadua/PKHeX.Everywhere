using AwesomeAssertions;

namespace PKHeX.Facade.Tests;

public class GameTests
{
    [Fact]
    public void Game_Gen4_ShouldUseEmulatorHandlerWhenExporting()
    {
        // Desmume save files has a footer that should be kept, otherwise, when opening a game
        // The emulator will blank out the whole save file making it invalid, loosing data
        File.ReadAllBytes(SaveFilePath.HgSs)
            .AsSpan()
            .Slice(0x080000)
            .ToArray()
            .Should().NotBeEmpty();
        
        var game = Game.LoadFrom(SaveFilePath.HgSs);
        game.SaveAndReload(savedGame =>
        {
            Span<byte> savedBytes = savedGame.ToByteArray();
            var savedFooter = savedBytes.Slice(0x080000);
            savedFooter.ToArray().Should().NotBeEmpty();
        });
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, new[] { "Ruby", "Sapphire", "Emerald", "FireRed", "LeafGreen" }, "Emerald")]
    [InlineData(SaveFilePath.HgSs, new[] { "HeartGold", "SoulSilver" }, "SoulSilver")]
    public void AvailableVersions_IncludeTheSaveVersionApproximation(string saveFile, string[] versions, string approximation)
    {
        var game = Game.LoadFrom(saveFile);

        game.AvailableVersions.Select(v => v.Name).Should().Contain(versions);
        game.AvailableVersions.Should().Contain(game.GameVersionApproximation);
        game.GameVersionApproximation.Name.Should().Be(approximation);
    }
}
