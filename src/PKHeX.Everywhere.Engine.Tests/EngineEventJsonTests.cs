using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class EngineEventJsonTests
{
    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";

    private static byte[] TestPlugIn => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{TestPlugInId}.dll"));

    private static List<JsonNode> Sent(Session session)
    {
        var sent = new List<JsonNode>();
        session.Published += engineEvent => sent.Add(JsonNode.Parse(session.Serialize(engineEvent)!)!);
        return sent;
    }

    [Fact]
    public void AnItemChangeIsSentWithItsCount()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var (at, _) = AddableItem(session.Game!)!.Value;
        var sent = Sent(session);

        Value(Dispatch(session, "inventory.setItem", Args(at, 2)));

        sent.Should().ContainSingle().Which.ToJsonString()
            .Should().Be($$"""{"type":"itemChanged","itemId":{{at.ItemId}},"count":2}""");
    }

    [Fact]
    public void AnExportIsSentWithTheExportedGame()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var game = session.Game!;
        var sent = Sent(session);

        Value(Dispatch(session, "game.export", "[]"));

        var expected = new
        {
            type = "gameExported",
            game = new
            {
                version = game.GameVersionApproximation.Name,
                versionId = game.GameVersionApproximation.Id,
                generation = game.Generation.ToString(),
                generationId = (int)game.Generation,
                trainerGender = game.Trainer.Gender.Name,
                boxCount = game.Trainer.PokemonBox.All.Count,
                party = game.Trainer.Party.Pokemons.Select(p => new { speciesId = p.Species.Id, species = p.Species.Name, level = p.Level }),
            },
        };
        sent.Should().ContainSingle().Which.ToJsonString().Should().Be(JsonSerializer.Serialize(expected));
        game.Trainer.Party.Pokemons.Should().NotBeEmpty();
    }

    [Fact]
    public void AnAddedEncounterIsSentWithThePokemon()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var row = Value(Dispatch(session, "encounters.search", Args((int)GameVersion.E, (int)Species.Abra)))!.AsArray()[^1]!;
        var sent = Sent(session);

        var added = Value(Dispatch(session, "box.addEncounter", Args(row["index"]!.GetValue<int>())))!;

        var at = added["at"]!;
        var pokemon = session.Game!.Trainer.PokemonBox.All[at["box"]!.GetValue<int>() * session.Game.SaveFile.BoxSlotCount + at["slot"]!.GetValue<int>()];
        var expected = new
        {
            type = "pokemonAdded",
            at,
            source = "encounter",
            pokemon = new
            {
                speciesId = pokemon.Species.Id,
                species = pokemon.Species.Name,
                gender = pokemon.Gender.Name,
                ball = pokemon.Ball.Name,
                level = pokemon.Level,
            },
        };
        sent.Should().ContainSingle().Which.ToJsonString().Should().Be(JsonSerializer.Serialize(expected));
        pokemon.Species.Name.Should().Be("Abra");
    }

    [Fact]
    public void ACommittedPokemonIsSentWithThePokemon()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.Party(0))));
        Value(Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.Draft(), 42)));
        var sent = Sent(session);

        Value(Dispatch(session, "pokemon.commit", "[]"));

        var pokemon = session.Game!.Trainer.Party.Pokemons[0];
        var expected = new
        {
            type = "pokemonSaved",
            at = new { source = "party", slot = 0, box = (int?)null },
            pokemon = new
            {
                speciesId = pokemon.Species.Id,
                species = pokemon.Species.Name,
                gender = pokemon.Gender.Name,
                ball = pokemon.Ball.Name,
                level = 42,
            },
        };
        sent.Should().ContainSingle().Which.ToJsonString().Should().Be(JsonSerializer.Serialize(expected));
    }

    [Fact]
    public void AnInstallIsSentFromThePlugInAssembly()
    {
        var session = new Session();
        _ = new PlugInHost(session);
        var sent = Sent(session);

        Value(Dispatch(session, "plugins.register", Args(Convert.ToBase64String(TestPlugIn), null!)));

        sent.Should().ContainSingle().Which["type"]!.GetValue<string>().Should().Be("plugInInstalled");
        sent[0]["plugInId"]!.GetValue<string>().Should().Be(TestPlugInId);
    }

    [Fact]
    public void ARunHookIsSentWithItsOutcome()
    {
        var session = Loaded(SaveFilePath.Emerald);
        new PlugInHost(session).Register(TestPlugIn);
        var sent = Sent(session);

        Value(Dispatch(session, "plugins.run", Args($"{TestPlugInId}.Greet", null!)));

        var ran = sent.Should().ContainSingle().Subject;
        ran["type"]!.GetValue<string>().Should().Be("plugInRan");
        ran["hookId"]!.GetValue<string>().Should().Be($"{TestPlugInId}.Greet");
        ran["outcome"]!["kind"]!.GetValue<string>().Should().Be("notify");
        ran["outcome"]!["message"]!.GetValue<string>().Should().Be($"Hello, {session.Game!.Trainer.Name}");
        ran["failure"].Should().BeNull();
    }

    [Fact]
    public void AFailedHookIsSentEvenThoughTheCommandFails()
    {
        var session = Loaded(SaveFilePath.Emerald);
        new PlugInHost(session).Register(TestPlugIn);
        var sent = Sent(session);

        Error(Dispatch(session, "plugins.run", Args($"{TestPlugInId}.Fail", null!))).Should().Be(ErrorCodes.PlugInFailed);

        var ran = sent.Should().ContainSingle().Subject;
        ran["outcome"].Should().BeNull();
        ran["failure"]!.ToJsonString().Should().Be("""{"type":"InvalidOperationException","message":"Failed on purpose"}""");
    }
}
