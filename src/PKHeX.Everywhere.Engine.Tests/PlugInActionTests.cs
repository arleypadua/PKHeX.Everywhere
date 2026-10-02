using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInActionTests
{
    private const string Nuzlocking = "PKHeX.Web.Plugins.Nuzlocking";
    private const string MaxRareCandies = $"{Nuzlocking}.MaxRareCandies";
    private const string Edge = $"{Nuzlocking}.EdgeLevelClick";
    private const string ZeroEvs = $"{Nuzlocking}.SetAllEVsToZero";

    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";
    private const string Greet = $"{TestPlugInId}.Greet";
    private const string Fail = $"{TestPlugInId}.Fail";
    private const string Unavailable = $"{TestPlugInId}.Unavailable";
    private const string Awaits = $"{TestPlugInId}.Awaits";

    private static readonly PokemonHandle FirstInParty = PokemonHandle.Party(0);

    private static byte[] PlugInBytes(string name) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{name}.dll"));

    private static (Session Session, PlugInHost Host, List<PlugInRan> Ran) Hosted(string plugIn)
    {
        var session = Loaded(SaveFilePath.Emerald);
        var host = new PlugInHost(session);
        var ran = PlugInRuns(session);
        host.Register(PlugInBytes(plugIn));
        return (session, host, ran);
    }

    private static string Call(params object?[] args) => JsonSerializer.Serialize(args.Select(arg => arg is PokemonHandle at
        ? (object)new { source = at.Source.ToString().ToLowerInvariant(), slot = at.Slot, box = at.Box }
        : arg));

    private static JsonArray Actions(Session session, string placement, PokemonHandle? target = null) =>
        Value(Dispatch(session, "plugins.actions", Call(placement, target)))!.AsArray();

    private static string Run(Session session, string id, PokemonHandle? target = null) =>
        Dispatch(session, "plugins.run", Call(id, target));

    [Fact]
    public void ListsNuzlockingsActionsPerPlacement()
    {
        var (session, host, _) = Hosted(Nuzlocking);
        host.SetToggle(Nuzlocking, ZeroEvs, true);

        Actions(session, "quick").Should().ContainSingle().Which.ToJsonString().Should().Be(new JsonObject
        {
            ["id"] = MaxRareCandies,
            ["plugInId"] = Nuzlocking,
            ["label"] = "Max Rare Candies",
            ["description"] = "Adds a button on the home page to give max rare candies",
            ["disabled"] = false,
            ["reason"] = null,
        }.ToJsonString());

        var edit = Actions(session, "pokemon", FirstInParty).Should().ContainSingle().Subject!;
        edit["id"]!.GetValue<string>().Should().Be(Edge);
        edit["label"]!.GetValue<string>().Should().Be("Edge");

        var stats = Actions(session, "pokemonStats", FirstInParty).Should().ContainSingle().Subject!;
        stats["id"]!.GetValue<string>().Should().Be(ZeroEvs);
        stats["label"]!.GetValue<string>().Should().Be("Zero all EVs");
    }

    [Fact]
    public void EdgesDescriptionFollowsItsSettings()
    {
        var (session, host, _) = Hosted(Nuzlocking);

        Actions(session, "pokemon", FirstInParty)[0]!["description"]!.GetValue<string>()
            .Should().Be("Will edge a pokemon to the previous level");

        host.UpdateSetting(Nuzlocking, "EdgeOnPreviousLevel", new Settings.SettingValue.BooleanValue(false));

        Actions(session, "pokemon", FirstInParty)[0]!["description"]!.GetValue<string>()
            .Should().Be("Will edge a pokemon to the current level");
    }

    [Fact]
    public void LeavesOutToggledOffHooksAndDisabledPlugIns()
    {
        var (session, host, _) = Hosted(Nuzlocking);

        Actions(session, "pokemonStats", FirstInParty).Should().BeEmpty();

        host.SetToggle(Nuzlocking, MaxRareCandies, false);
        Actions(session, "quick").Should().BeEmpty();

        host.SetEnabled(Nuzlocking, false);
        Actions(session, "pokemon", FirstInParty).Should().BeEmpty();
    }

    [Fact]
    public async Task RunsAnActionThatAwaitsWithoutBlocking()
    {
        var (session, host, _) = Hosted(TestPlugInId);
        host.SetToggle(TestPlugInId, Awaits, true);
        var release = new TaskCompletionSource();
        host.Find(TestPlugInId)!.Assembly.GetType(Awaits)!.GetProperty("Gate")!.SetValue(null, release.Task);

        var running = Dispatcher.Dispatch(session, "plugins.run", Call(Awaits, null));
        running.IsCompleted.Should().BeFalse();

        release.SetResult();
        Value(await running)!["message"]!.GetValue<string>().Should().Be("Awaited");
    }

    [Fact]
    public async Task ReportsOtherChangesWhileAnActionAwaits()
    {
        var (session, host, _) = Hosted(TestPlugInId);
        host.SetToggle(TestPlugInId, Awaits, true);
        var release = new TaskCompletionSource();
        host.Find(TestPlugInId)!.Assembly.GetType(Awaits)!.GetProperty("Gate")!.SetValue(null, release.Task);
        var changed = new List<string>();
        session.Changed += changed.AddRange;

        var running = Dispatcher.Dispatch(session, "plugins.run", Call(Awaits, null));
        session.Invalidate(Topics.Box);

        changed.Should().Equal(Topics.Box);
        release.SetResult();
        await running;
    }

    [Fact]
    public void PokemonPlacementsNeedATarget()
    {
        var (session, _, _) = Hosted(Nuzlocking);

        Error(Dispatch(session, "plugins.actions", Call("pokemon", null))).Should().Be(ErrorCodes.BadArguments);
        Error(Dispatch(session, "plugins.actions", Call("pokemon", PokemonHandle.Party(6)))).Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public void ReportsADisabledActionWithoutAReasonAsDisabled()
    {
        var (session, host, _) = Hosted(TestPlugInId);
        host.SetToggle(TestPlugInId, Unavailable, true);

        var action = Actions(session, "quick").Single(a => a!["id"]!.GetValue<string>() == Unavailable)!;

        action["disabled"]!.GetValue<bool>().Should().BeTrue();
        action["reason"].Should().BeNull();
    }

    [Fact]
    public void RefusesToRunADisabledAction()
    {
        var (session, host, ran) = Hosted(TestPlugInId);
        host.SetToggle(TestPlugInId, Unavailable, true);

        Error(Run(session, Unavailable)).Should().Be(ErrorCodes.BadArguments);
        ran.Should().BeEmpty();
    }

    [Fact]
    public void RefusesToRunAnUnknownOrToggledOffAction()
    {
        var (session, host, _) = Hosted(TestPlugInId);
        host.SetToggle(TestPlugInId, Greet, false);

        Error(Run(session, Greet)).Should().Be(ErrorCodes.NotFound);
        Error(Run(session, "Nope")).Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public void MaxRareCandiesFillsTheRareCandiesAndInvalidatesEverything()
    {
        var (session, _, ran) = Hosted(Nuzlocking);
        var invalidated = new List<string[]>();
        session.Changed += invalidated.Add;

        var outcome = Value(Run(session, MaxRareCandies))!;

        var rareCandy = Value(Dispatch(session, "inventory.get", "[]"))!.AsArray()
            .SelectMany(pouch => pouch!["items"]!.AsArray())
            .Single(item => item!["name"]!.GetValue<string>() == "Rare Candy")!;
        var count = rareCandy["count"]!.GetValue<int>();
        count.Should().BeGreaterThan(1).And.Be(rareCandy["maxCount"]!.GetValue<int>());
        outcome.ToJsonString().Should().Be(new JsonObject
        {
            ["kind"] = "notify",
            ["message"] = $"{count} Rare candies set",
            ["description"] = null,
            ["type"] = "none",
            ["path"] = null,
        }.ToJsonString());
        invalidated.Should().ContainSingle().Which.Should().Contain(Topics.All);
        ran.Should().ContainSingle().Which.HookId.Should().Be(MaxRareCandies);
    }

    [Fact]
    public void EdgeLowersThePokemonToTheEdgeOfItsPreviousLevel()
    {
        var (session, _, _) = Hosted(Nuzlocking);
        var level = session.Game!.SaveFile.GetPartySlotAtIndex(0).CurrentLevel;

        var outcome = Value(Run(session, Edge, FirstInParty))!;

        var edged = session.Game.SaveFile.GetPartySlotAtIndex(0);
        edged.CurrentLevel.Should().Be((byte)(level - 1));
        outcome["kind"]!.GetValue<string>().Should().Be("notify");
        outcome["message"]!.GetValue<string>().Should().Be("Pokemon experience changed");
        outcome["description"]!.GetValue<string>().Should().EndWith($"to {edged.EXP}");
        outcome["type"]!.GetValue<string>().Should().Be("success");
    }

    [Fact]
    public void ZeroAllEvsWritesTheSave()
    {
        var (session, host, _) = Hosted(Nuzlocking);
        host.SetToggle(Nuzlocking, ZeroEvs, true);
        var party = session.Game!.Trainer.Party;
        var pokemon = party.Pokemons[0];
        pokemon.EVs.Attack = 10;
        pokemon.EVs.Speed = 20;
        party.Commit();

        Value(Run(session, ZeroEvs, FirstInParty))!["type"]!.GetValue<string>().Should().Be("info");

        var saved = session.Game.SaveFile.GetPartySlotAtIndex(0);
        new[] { saved.EV_HP, saved.EV_ATK, saved.EV_DEF, saved.EV_SPA, saved.EV_SPD, saved.EV_SPE }.Should().OnlyContain(ev => ev == 0);
    }

    [Fact]
    public void AThrowingActionReturnsAnErrorAndIsPublished()
    {
        var (session, host, ran) = Hosted(TestPlugInId);

        Error(Run(session, Fail)).Should().Be(ErrorCodes.PlugInFailed);

        ran.Should().ContainSingle().Which.Failure!.Type.Should().Be(nameof(InvalidOperationException));
        host.Failures.Should().ContainSingle().Which.HookId.Should().Be(Fail);
    }

    [Fact]
    public void ReturnsAPageOutcome()
    {
        var (session, host, _) = Hosted(TestPlugInId);
        host.SetToggle(TestPlugInId, $"{TestPlugInId}.OpenHello", true);

        var outcome = Value(Run(session, $"{TestPlugInId}.OpenHello"))!;

        outcome["kind"]!.GetValue<string>().Should().Be("openPage");
        outcome["path"]!.GetValue<string>().Should().Be("hello");
    }

    [Fact]
    public void ChangingWhatActionsAreListedInvalidatesEverything()
    {
        var (session, host, _) = Hosted(Nuzlocking);
        var invalidated = new List<string[]>();
        session.Changed += invalidated.Add;

        host.UpdateSetting(Nuzlocking, "EdgeOnPreviousLevel", new Settings.SettingValue.BooleanValue(false));
        host.SetToggle(Nuzlocking, ZeroEvs, true);
        host.SetEnabled(Nuzlocking, false);
        host.Unregister(Nuzlocking);
        host.Register(PlugInBytes(Nuzlocking));

        invalidated.Should().HaveCount(5).And.OnlyContain(topics => topics.SequenceEqual(new[] { Topics.All }));
    }
}
