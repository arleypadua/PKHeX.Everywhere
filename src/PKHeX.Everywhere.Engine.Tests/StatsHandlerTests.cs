using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class StatsHandlerTests
{
    private const string Nuzlocking = "PKHeX.Web.Plugins.Nuzlocking";
    private const string ZeroEvs = $"{Nuzlocking}.SetAllEVsToZero";

    private static readonly PokemonHandle Draft = PokemonHandle.Draft();

    [Theory]
    [SupportedSaveFiles]
    public void DetailsReturnTheStatInputsAndTheComputedStats(string saveFile)
    {
        var session = Loaded(saveFile);
        var pkm = session.Game!.Trainer.Party.Pokemons[0].Pkm;
        var stats = pkm.GetStats(pkm.PersonalInfo);

        var details = Details(session, PokemonHandle.Party(0));

        details["ivs"]!.ToJsonString().Should().Be(Stats(pkm.IV_HP, pkm.IV_ATK, pkm.IV_DEF, pkm.IV_SPA, pkm.IV_SPD, pkm.IV_SPE));
        details["evs"]!.ToJsonString().Should().Be(Stats(pkm.EV_HP, pkm.EV_ATK, pkm.EV_DEF, pkm.EV_SPA, pkm.EV_SPD, pkm.EV_SPE));
        details["stats"]!.ToJsonString().Should().Be(Stats(stats[0], stats[1], stats[2], stats[4], stats[5], stats[3]));
        if (pkm is IAwakened awakened)
            details["avs"]!.ToJsonString().Should().Be(Stats(awakened.AV_HP, awakened.AV_ATK, awakened.AV_DEF, awakened.AV_SPA, awakened.AV_SPD, awakened.AV_SPE));
        else
            details["avs"].Should().BeNull();
    }

    [Fact]
    public void UpdateAppliesTheStatInputsToTheDraftAndRecomputesTheStats()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));
        Update(session, Draft, new { ivs = new { attack = 0 }, evs = new { attack = 0 } });
        var before = Details(session, Draft)["stats"]!["attack"]!.GetValue<int>();

        Update(session, Draft, new { ivs = new { attack = 31 }, evs = new { attack = 252 } });

        var details = Details(session, Draft);
        details["ivs"]!["attack"]!.GetValue<int>().Should().Be(31);
        details["evs"]!["attack"]!.GetValue<int>().Should().Be(252);
        details["stats"]!["attack"]!.GetValue<int>().Should().BeGreaterThan(before);
        Details(session, PokemonHandle.Party(0))["evs"]!["attack"]!.GetValue<int>().Should().NotBe(252);
    }

    [Fact]
    public void UpdateAppliesTheAwakeningValuesAndCombatPower()
    {
        var session = Loaded(SaveFilePath.LetsGoPikachu);
        Edit(session, PokemonHandle.Party(0));

        Update(session, Draft, new { avs = new { speed = 150 } });
        var calculated = Details(session, Draft)["calculatedCombatPower"]!.GetValue<int>();
        Update(session, Draft, new { combatPower = 1234 });

        var details = Details(session, Draft);
        details["avs"]!["speed"]!.GetValue<int>().Should().Be(150);
        details["combatPower"]!.GetValue<int>().Should().Be(1234);
        details["calculatedCombatPower"]!.GetValue<int>().Should().Be(calculated);
    }

    [Theory]
    [InlineData(SaveFilePath.HgSs, "ivs", "attack", 32, "Attack IV")]
    [InlineData(SaveFilePath.Emerald, "evs", "speed", 256, "Speed EV")]
    [InlineData(SaveFilePath.HgSs, "avs", "health", 1, "Awakening values")]
    [InlineData(SaveFilePath.LetsGoPikachu, "avs", "defense", 201, "Defense AV")]
    public void UpdateRejectsAStatTheSaveCantHoldNamingIt(string saveFile, string field, string stat, int value, string named)
    {
        var session = Loaded(saveFile);
        Edit(session, PokemonHandle.Party(0));
        var before = Details(session, Draft);
        var patch = new Dictionary<string, object> { [field] = new Dictionary<string, int> { [stat] = value }, ["nickname"] = "Sparky" };

        var result = JsonNode.Parse(Dispatch(session, "pokemon.update", Args(Draft, patch)))!;

        result["error"]!["code"]!.GetValue<string>().Should().Be("invalid-patch");
        result["error"]!["message"]!.GetValue<string>().Should().StartWith(named);
        Details(session, Draft).ToJsonString().Should().Be(before.ToJsonString());
    }

    [Fact]
    public void UpdateRejectsCombatPowerOutsideLetsGo()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Error(Dispatch(session, "pokemon.update", Args(Draft, new { combatPower = 100 }))).Should().Be("invalid-patch");
    }

    [Fact]
    public void StatsPlugInActionsChangeTheDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var host = new PlugInHost(session);
        host.Register(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{Nuzlocking}.dll")));
        host.SetToggle(Nuzlocking, ZeroEvs, true);
        var saved = Details(session, PokemonHandle.Party(0))["evs"]!.ToJsonString();
        Edit(session, PokemonHandle.Party(0));
        Update(session, Draft, new { evs = new { attack = 100 } });
        var before = Details(session, Draft)["stats"]!["attack"]!.GetValue<int>();

        Value(Dispatch(session, "plugins.run", Args(ZeroEvs, Draft)));

        var details = Details(session, Draft);
        details["evs"]!.ToJsonString().Should().Be(Stats(0, 0, 0, 0, 0, 0));
        details["stats"]!["attack"]!.GetValue<int>().Should().BeLessThan(before);
        Details(session, PokemonHandle.Party(0))["evs"]!.ToJsonString().Should().Be(saved);
    }

    private static string Stats(int health, int attack, int defense, int specialAttack, int specialDefense, int speed) =>
        new JsonObject
        {
            ["health"] = health,
            ["attack"] = attack,
            ["defense"] = defense,
            ["specialAttack"] = specialAttack,
            ["specialDefense"] = specialDefense,
            ["speed"] = speed,
        }.ToJsonString();

    private static void Edit(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.edit", Args(at)));

    private static void Update(Session session, PokemonHandle at, object patch) => Value(Dispatch(session, "pokemon.update", Args(at, patch)));

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;
}
