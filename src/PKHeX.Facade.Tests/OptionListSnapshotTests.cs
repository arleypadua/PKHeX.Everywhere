using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

// Each list is stored as its count and a hash of its ids and names. Run with UPDATE_SNAPSHOTS=1 to rewrite the files.
public class OptionListSnapshotTests
{
    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.LetsGoEevee)]
    public void OptionListsOfOfficialSavesDontChange(string saveFile)
    {
        var actual = Snapshot(SaveFilePath.Load(saveFile));
        var path = SnapshotPath(saveFile);

        if (Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "1") File.WriteAllText(path, actual);

        actual.Should().Be(File.ReadAllText(path));
    }

    private static string Snapshot(Game game)
    {
        var lines = new List<string>
        {
            Line("natures", game.Options.Natures),
            Line("balls", game.Options.Balls),
            Line("languages", game.Options.Languages),
            Line("heldItems", game.Options.HeldItems),
            Line("originGames", game.Options.OriginGames),
            Line("moves", game.Options.Moves),
            Line("species", game.SpeciesRepository.AllGameSpecies.OrderBy(s => s.Id).Select(s => new Choice(s.Id, s.Name))),
            Line("items", game.ItemRepository.GameItems.OrderBy(i => i.Id).Select(i => new Choice(i.Id, i.Name))),
            Line("locations", game.LocationRepository.Locations.Select(l => new Choice(l.Id, l.Name))),
            Line("eggLocations", game.LocationRepository.EggLocations.Select(l => new Choice(l.Id, l.Name))),
        };

        foreach (var (pokemon, slot) in game.Trainer.Party.Pokemons.Select((pokemon, slot) => (pokemon, slot)))
        {
            var options = pokemon.Options();
            lines.Add(Line($"party/{slot}/species", options.Species));
            lines.Add(Line($"party/{slot}/abilities", options.Abilities));
            lines.Add(Line($"party/{slot}/forms", options.Forms));
            lines.Add(Line($"party/{slot}/metLocations", options.MetLocations));
            lines.Add(Line($"party/{slot}/moves", options.Moves));
            lines.Add($"party/{slot}/locked {string.Join(',', options.Locked.Order())}".TrimEnd());
        }

        return string.Join('\n', lines) + '\n';
    }

    private static string Line(string name, IEnumerable<Choice> choices)
    {
        var list = choices.Select(choice => $"{choice.Id}:{choice.Name}").ToArray();
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', list))))[..16];
        return $"{name} {list.Length} {hash}";
    }

    private static string SnapshotPath(string saveFile, [CallerFilePath] string source = "") =>
        Path.Combine(Path.GetDirectoryName(source)!, "data", "options", $"{Path.GetFileNameWithoutExtension(saveFile)}.txt");
}
