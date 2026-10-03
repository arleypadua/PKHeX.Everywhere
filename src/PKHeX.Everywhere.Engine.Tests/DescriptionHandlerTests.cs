using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;

namespace PKHeX.Everywhere.Engine.Tests;

public class DescriptionHandlerTests
{
    private static readonly PokemonHandle Draft = PokemonHandle.Draft();

    [Theory]
    [SupportedSaveFiles]
    public void GameListsReturnWhatTheSaveAllows(string saveFile)
    {
        var session = Loaded(saveFile);
        var options = session.Game!.Options;

        List(session, "game.natures").Should().Be(Serialized(options.Natures));
        List(session, "game.balls").Should().Be(Serialized(options.Balls));
        List(session, "game.languages").Should().Be(Serialized(options.Languages));
        List(session, "game.heldItems").Should().Be(Serialized(options.HeldItems));
        List(session, "game.moves").Should().Be(Serialized(options.Moves));
    }

    [Theory]
    [InlineData("game.natures")]
    [InlineData("game.balls")]
    [InlineData("game.languages")]
    [InlineData("game.heldItems")]
    [InlineData("game.moves")]
    public void GameListsReturnNoSaveWithoutALoadedSave(string call) =>
        Error(Dispatch(new Session(), call, "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void DetailsReturnTheDescriptionFields(string saveFile)
    {
        var session = Loaded(saveFile);
        var pokemon = session.Game!.Trainer.Party.Pokemons[0];

        var details = Details(session, PokemonHandle.Party(0));

        details["species"]!.GetValue<int>().Should().Be(pokemon.Species.Id);
        details["heldItem"]!.GetValue<int>().Should().Be(pokemon.Pkm.HeldItem);
        details["friendship"]!.GetValue<int>().Should().Be(pokemon.Friendship);
        details["gender"]!.GetValue<string>().Should().Be(pokemon.Gender.Name.ToLowerInvariant());
        details["isShiny"]!.GetValue<bool>().Should().Be(pokemon.IsShiny);
        details["isAlpha"].Should().BeNull();
    }

    [Fact]
    public void UpdateAppliesSpeciesBeforeFormBeforeAbilityOnTheDraft()
    {
        var session = Loaded(SaveFilePath.LetsGoPikachu);
        Edit(session, PokemonHandle.Party(0));

        Update(session, Draft, new { species = (int)Species.Vulpix, form = 1, ability = (int)Ability.SnowCloak, gender = "female", nature = (int)Nature.Timid });

        var details = Details(session, Draft);
        details["species"]!.GetValue<int>().Should().Be((int)Species.Vulpix);
        details["form"]!.GetValue<int>().Should().Be(1);
        details["ability"]!.GetValue<int>().Should().Be((int)Ability.SnowCloak);
        details["gender"]!.GetValue<string>().Should().Be("female");
        details["nature"]!.GetValue<int>().Should().Be((int)Nature.Timid);
        Details(session, PokemonHandle.Party(0))["species"]!.GetValue<int>().Should().NotBe((int)Species.Vulpix);
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, "ball", 20, "Ball")]
    [InlineData(SaveFilePath.Crystal, "species", 252, "Species")]
    [InlineData(SaveFilePath.HgSs, "nature", 3, "Nature")]
    [InlineData(SaveFilePath.HgSs, "isAlpha", true, "Alpha")]
    [InlineData(SaveFilePath.HgSs, "heldItem", 5000, "Held item")]
    [InlineData(SaveFilePath.HgSs, "language", 9, "Language")]
    public void UpdateRejectsAValueTheSaveCantHoldNamingTheField(string saveFile, string field, object value, string named)
    {
        var session = Loaded(saveFile);
        Edit(session, PokemonHandle.Party(0));
        var before = Details(session, Draft);

        var result = JsonNode.Parse(Dispatch(session, "pokemon.update", Args(Draft, new Dictionary<string, object> { [field] = value, ["nickname"] = "Sparky" })))!;

        result["error"]!["code"]!.GetValue<string>().Should().Be("invalid-patch");
        result["error"]!["message"]!.GetValue<string>().Should().StartWith(named);
        Details(session, Draft).ToJsonString().Should().Be(before.ToJsonString());
    }

    [Theory]
    [SupportedSaveFiles]
    public void OptionsReturnTheFacadeOptions(string saveFile)
    {
        var session = Loaded(saveFile);
        var expected = session.Game!.Trainer.Party.Pokemons[0].Options();

        var options = Options(session, PokemonHandle.Party(0));

        options["species"]!.ToJsonString().Should().Be(Serialized(expected.Species));
        options["abilities"]!.ToJsonString().Should().Be(Serialized(expected.Abilities));
        options["forms"]!.ToJsonString().Should().Be(Serialized(expected.Forms));
        options["moves"]!.ToJsonString().Should().Be(Serialized(expected.Moves));
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, new[] { "nature" })]
    [InlineData(SaveFilePath.FireRed, new[] { "nature" })]
    [InlineData(SaveFilePath.HgSs, new[] { "nature" })]
    [InlineData(SaveFilePath.LetsGoPikachu, new string[0])]
    [InlineData(SaveFilePath.LetsGoEevee, new string[0])]
    public void OptionsLockTheNatureWhereItComesFromThePid(string saveFile, string[] locked)
    {
        var session = Loaded(saveFile);

        foreach (var pokemon in session.Game!.Trainer.Party.Pokemons.Select((_, slot) => PokemonHandle.Party(slot)))
            Options(session, pokemon)["locked"]!.AsArray().Select(field => field!.GetValue<string>()).Should().Equal(locked);
    }

    [Fact]
    public void OptionsFollowTheDraftAfterAnUpdate()
    {
        var session = Loaded(SaveFilePath.LetsGoPikachu);
        var at = Enumerable.Range(0, session.Game!.Trainer.Party.Pokemons.Count)
            .Select(PokemonHandle.Party)
            .First(at => session.Game.Trainer.Party.Pokemons[at.Slot] is { Species.Species: not Species.Vulpix, Form.HasForm: false });
        Edit(session, at);
        Options(session, Draft)["forms"]!.AsArray().Should().BeEmpty();
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Update(session, Draft, new { species = (int)Species.Vulpix });

        changes.Should().ContainSingle().Which.Should().Contain(Topics.Draft);
        Options(session, Draft)["forms"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()).Should().Contain("Alola");
        Options(session, Draft)["abilities"]!.AsArray().Select(a => a!["id"]!.GetValue<int>()).Should().Contain((int)Ability.FlashFire);
    }

    [Fact]
    public void TheAlphaFlagIsNullWithoutAlphaPokemonAndSetWithThem()
    {
        var session = new Session();
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA));
        session.Load(game, "legends.sav");
        var file = new Pokemon(new PA8 { Species = (ushort)Species.Pikachu }, game).ToFile().Bytes;
        var added = Value(Dispatch(session, "box.addFromFile", Args(Convert.ToBase64String(file))))!;
        Value(Dispatch(session, "pokemon.edit", $"[{added["at"]!.ToJsonString()}]"));
        Details(session, Draft)["isAlpha"]!.GetValue<bool>().Should().BeFalse();

        Update(session, Draft, new { isAlpha = true });

        Details(session, Draft)["isAlpha"]!.GetValue<bool>().Should().BeTrue();
    }

    private static string List(Session session, string call) => Value(Dispatch(session, call, "[]"))!.ToJsonString();

    private static string Serialized(IEnumerable<Facade.Pokemons.Choice> choices) =>
        JsonSerializer.Serialize(choices.Select(c => new { id = c.Id, name = c.Name }));

    private static void Edit(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.edit", Args(at)));

    private static void Update(Session session, PokemonHandle at, object patch) => Value(Dispatch(session, "pokemon.update", Args(at, patch)));

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static JsonNode Options(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.options", Args(at)))!;
}
