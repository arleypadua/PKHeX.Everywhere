using FluentAssertions;

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
}