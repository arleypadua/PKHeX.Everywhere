using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class DraftHandlerTests
{
    private const string LevelUp = "PKHeX.Everywhere.Engine.Tests.PlugIn.LevelUp";
    private static readonly PokemonHandle Draft = PokemonHandle.Draft();

    public static TheoryData<string, bool> SavesAndSlots()
    {
        var data = new TheoryData<string, bool>();
        foreach (var saveFile in new SupportedSaveFilesAttribute().GetData(null!).Select(d => (string)d[0]))
        {
            data.Add(saveFile, false);
            data.Add(saveFile, true);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(SavesAndSlots))]
    public void EditOpensACopyOfTheSavedPokemon(string saveFile, bool inBox)
    {
        var session = Loaded(saveFile);
        var at = Slot(session, inBox);

        Edit(session, at);

        Details(session, Draft).ToJsonString().Should().Be(Details(session, at).ToJsonString());
        Get(session, Draft)["id"]!.ToJsonString().Should().Be(Get(session, at)["id"]!.ToJsonString());
    }

    [Theory]
    [MemberData(nameof(SavesAndSlots))]
    public void ChangesToTheDraftReachTheSaveOnlyOnCommit(string saveFile, bool inBox)
    {
        var session = Loaded(saveFile);
        var at = Slot(session, inBox);
        var saved = Details(session, at);
        var level = saved["level"]!.GetValue<int>() == 50 ? 51 : 50;

        Edit(session, at);
        Update(session, Draft, new { nickname = "Sparky", level });

        Details(session, at).ToJsonString().Should().Be(saved.ToJsonString());
        Details(session, Draft)["nickname"]!.GetValue<string>().Should().Be("Sparky");

        var id = Value(Dispatch(session, "pokemon.commit", "[]"))!.GetValue<string>();

        var committed = Details(session, at);
        committed["nickname"]!.GetValue<string>().Should().Be("Sparky");
        committed["level"]!.GetValue<int>().Should().Be(level);
        id.Should().Be(Get(session, at)["id"]!.GetValue<string>());
        session.Game!.SaveAndReload(reloaded =>
        {
            var pokemon = inBox ? reloaded.Trainer.PokemonBox.All[BoxIndex(reloaded, at)] : reloaded.Trainer.Party.Pokemons[at.Slot];
            pokemon.Nickname.Should().Be("Sparky");
            pokemon.Level.Should().Be(level);
        });
    }

    [Fact]
    public void CommitReturnsTheNewIdWhenTheChangeGivesThePokemonANewOne()
    {
        var session = Loaded(SaveFilePath.Crystal);
        var before = Get(session, PokemonHandle.Party(0))["id"]!.GetValue<string>();
        Edit(session, PokemonHandle.Party(0));
        Update(session, Draft, new { level = Details(session, Draft)["level"]!.GetValue<int>() == 50 ? 51 : 50 });

        var id = Value(Dispatch(session, "pokemon.commit", "[]"))!.GetValue<string>();

        id.Should().NotBe(before).And.Be(Get(session, PokemonHandle.Party(0))["id"]!.GetValue<string>());
    }

    [Fact]
    public void CommitClearsTheDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Value(Dispatch(session, "pokemon.commit", "[]"));

        Error(Dispatch(session, "pokemon.details", Args(Draft))).Should().Be("not-found");
        Error(Dispatch(session, "pokemon.commit", "[]")).Should().Be("not-found");
    }

    [Fact]
    public void EditReplacesTheOpenDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (inBox, _) = FirstBoxPokemon(session.Game!)!.Value;
        Edit(session, PokemonHandle.Party(0));
        Update(session, Draft, new { nickname = "Sparky" });

        Edit(session, inBox);

        Details(session, Draft).ToJsonString().Should().Be(Details(session, inBox).ToJsonString());
        Value(Dispatch(session, "pokemon.commit", "[]"));
        Details(session, PokemonHandle.Party(0))["nickname"]!.GetValue<string>().Should().NotBe("Sparky");
    }

    [Fact]
    public void LoadingASaveClearsTheDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.Emerald)), "emerald.sav")));

        Error(Dispatch(session, "pokemon.details", Args(Draft))).Should().Be("not-found");
    }

    [Fact]
    public void ExistingPokemonCallsWorkOnTheDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Value(Dispatch(session, "pokemon.setLevel", Args(Draft, 42)));

        Get(session, Draft)["level"]!.GetValue<int>().Should().Be(42);
        Value(Dispatch(session, "pokemon.showdown", Args(Draft)))!.GetValue<string>().Should().Contain("Level: 42");
        Get(session, PokemonHandle.Party(0))["level"]!.GetValue<int>().Should().NotBe(42);
    }

    [Fact]
    public void EditRejectsTheDraftHandle()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Error(Dispatch(session, "pokemon.edit", Args(Draft))).Should().Be("draft-not-allowed");
    }

    [Fact]
    public void DraftCallsReturnNotFoundWithoutADraft()
    {
        var session = Loaded(SaveFilePath.HgSs);

        Error(Dispatch(session, "pokemon.details", Args(Draft))).Should().Be("not-found");
        Error(Dispatch(session, "pokemon.update", Args(Draft, new { level = 5 }))).Should().Be("not-found");
        Error(Dispatch(session, "pokemon.commit", "[]")).Should().Be("not-found");
    }

    [Theory]
    [SupportedSaveFiles]
    public void DetailsReturnsNicknameLevelAndLegality(string saveFile)
    {
        var session = Loaded(saveFile);
        var pokemon = session.Game!.Trainer.Party.Pokemons[0];

        var details = Details(session, PokemonHandle.Party(0));

        details["nickname"]!.GetValue<string>().Should().Be(pokemon.Nickname);
        details["level"]!.GetValue<int>().Should().Be(pokemon.Level);
        details["legality"]!["valid"]!.GetValue<bool>().Should().Be(details["legality"]!["messages"]!.AsArray().Count == 0);
    }

    [Theory]
    [SupportedSaveFiles]
    public void UpdateChangesASavedPokemon(string saveFile)
    {
        var session = Loaded(saveFile);

        Update(session, PokemonHandle.Party(0), new { nickname = "Sparky" });

        session.Game!.SaveAndReload(reloaded => reloaded.Trainer.Party.Pokemons[0].Nickname.Should().Be("Sparky"));
    }

    [Theory]
    [MemberData(nameof(SavesAndSlots))]
    public void UpdateChangesTheOriginalTrainer(string saveFile, bool inBox)
    {
        var session = Loaded(saveFile);
        var at = Slot(session, inBox);
        Edit(session, at);

        Update(session, Draft, new { trainerId = 12345, originalTrainerName = "Red", originalTrainerGender = "male" });

        var details = Details(session, Draft);
        details["trainerId"]!.GetValue<uint>().Should().Be(12345);
        details["originalTrainerName"]!.GetValue<string>().Should().Be("Red");
        details["originalTrainerGender"]!.GetValue<string>().Should().Be("male");
        Value(Dispatch(session, "pokemon.commit", "[]"));
        session.Game!.SaveAndReload(reloaded =>
        {
            var pokemon = inBox ? reloaded.Trainer.PokemonBox.All[BoxIndex(reloaded, at)] : reloaded.Trainer.Party.Pokemons[at.Slot];
            pokemon.Owner.TID.Should().Be(12345);
            pokemon.Owner.Name.Should().Be("Red");
            pokemon.Owner.Gender.Should().Be(PKHeX.Facade.Gender.Male);
        });
    }

    [Fact]
    public void UpdateChangesTheHandlingTrainer()
    {
        var session = Loaded(SaveFilePath.LetsGoPikachu);
        Edit(session, PokemonHandle.Party(0));

        Update(session, Draft, new { secretId = 1234, handlingTrainerName = "Blue", handlingTrainerGender = "female", currentHandler = "handlingTrainer" });

        var details = Details(session, Draft);
        details["secretId"]!.GetValue<uint>().Should().Be(1234);
        details["handlingTrainerName"]!.GetValue<string>().Should().Be("Blue");
        details["handlingTrainerGender"]!.GetValue<string>().Should().Be("female");
        details["currentHandler"]!.GetValue<string>().Should().Be("handlingTrainer");
    }

    [Fact]
    public void UpdateRejectsTrainerDetailsTheSaveCantStore()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Edit(session, PokemonHandle.Party(0));
        var before = Details(session, Draft);

        var result = JsonNode.Parse(Dispatch(session, "pokemon.update", Args(Draft, new { originalTrainerName = "Red", currentHandler = "handlingTrainer" })))!;

        result["error"]!["code"]!.GetValue<string>().Should().Be("invalid-patch");
        Details(session, Draft).ToJsonString().Should().Be(before.ToJsonString());
    }

    [Fact]
    public void UpdateRejectsTheWholePatchWhenAFieldCantBeStored()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));
        var before = Details(session, Draft);

        var result = JsonNode.Parse(Dispatch(session, "pokemon.update", Args(Draft, new { nickname = "Sparky", level = 101 })))!;

        result["error"]!["code"]!.GetValue<string>().Should().Be("invalid-patch");
        result["error"]!["message"]!.GetValue<string>().Should().StartWith("Level");
        Details(session, Draft).ToJsonString().Should().Be(before.ToJsonString());
    }

    [Fact]
    public void UpdateAppliesAnIllegalValueAndReportsIt()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Update(session, Draft, new { level = 1 });

        var details = Details(session, Draft);
        details["level"]!.GetValue<int>().Should().Be(1);
        details["legality"]!["valid"]!.GetValue<bool>().Should().BeFalse();
        details["legality"]!["messages"]!.AsArray().Should().NotBeEmpty();
    }

    [Fact]
    public void ChangesRaisePokemonChangedAndCommitRaisesPokemonSavedWithTheSlot()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (inBox, _) = FirstBoxPokemon(session.Game!)!.Value;
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Value(Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.Party(0), 42)));
        Update(session, PokemonHandle.Party(0), new { nickname = "Sparky" });
        Edit(session, inBox);
        Update(session, Draft, new { level = 42 });
        Value(Dispatch(session, "pokemon.setLevel", Args(Draft, 43)));
        Value(Dispatch(session, "pokemon.commit", "[]"));

        published.Should().Equal(
            new PokemonChanged(PokemonHandle.Party(0)),
            new PokemonChanged(PokemonHandle.Party(0)),
            new PokemonChanged(Draft),
            new PokemonChanged(Draft),
            new PokemonSaved(inBox));
    }

    [Fact]
    public void DraftCommandsReportTheDraftTopic()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Edit(session, PokemonHandle.Party(0));
        Update(session, Draft, new { level = 42 });
        Value(Dispatch(session, "pokemon.commit", "[]"));

        changes.Should().BeEquivalentTo(
            [new[] { Topics.Party, Topics.Draft }, new[] { Topics.Draft }, new[] { Topics.Draft, Topics.Party }],
            o => o.WithStrictOrdering());
    }

    [Fact]
    public void ChangeHooksRunOnTheDraftAndSaveHooksOnTheSlot()
    {
        var session = Loaded(SaveFilePath.Emerald);
        new PlugInHost(session).Register(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", "PKHeX.Everywhere.Engine.Tests.PlugIn.dll")));
        var saved = session.Game!.Trainer.Party.Pokemons[0].Nickname;
        Edit(session, PokemonHandle.Party(0));

        Update(session, Draft, new { level = 42 });

        Details(session, Draft)["nickname"]!.GetValue<string>().Should().Be("Changed");
        session.Game.SaveFile.GetPartySlotAtIndex(0).Nickname.Should().Be(saved);

        Value(Dispatch(session, "pokemon.commit", "[]"));

        session.Game.SaveFile.GetPartySlotAtIndex(0).Nickname.Should().Be("Saved");
    }

    [Fact]
    public void PlugInsReadAndUpdateTheDraft()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var host = new PlugInHost(session);
        host.Register(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", "PKHeX.Everywhere.Engine.Tests.PlugIn.dll")));
        host.SetToggle("PKHeX.Everywhere.Engine.Tests.PlugIn", LevelUp, true);
        Edit(session, PokemonHandle.Party(0));
        Value(Dispatch(session, "pokemon.setLevel", Args(Draft, 41)));

        var outcome = Value(Dispatch(session, "plugins.run", Args(LevelUp, Draft)))!;

        outcome["message"]!.GetValue<string>().Should().EndWith("is level 42");
        Details(session, Draft)["level"]!.GetValue<int>().Should().Be(42);
        Get(session, PokemonHandle.Party(0))["level"]!.GetValue<int>().Should().NotBe(42);
    }

    [Theory]
    [MemberData(nameof(SavesAndSlots))]
    public void ExportReturnsTheDraftWithUnsavedEdits(string saveFile, bool inBox)
    {
        var session = Loaded(saveFile);
        var at = Slot(session, inBox);
        Edit(session, at);
        Update(session, Draft, new { nickname = "Sparky" });

        var draft = Exported(session, Draft);
        var saved = Exported(session, at);

        draft.Pokemon.Nickname.Should().Be("Sparky");
        draft.FileName.Should().Be(draft.Pokemon.FileName);
        saved.Pokemon.Nickname.Should().Be(Details(session, at)["nickname"]!.GetValue<string>());
        saved.FileName.Should().Be(saved.Pokemon.FileName);
    }

    [Fact]
    public void PlugInActionChangesShowInTheDraftAfterTheRun()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var host = new PlugInHost(session);
        host.Register(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", "PKHeX.Everywhere.Engine.Tests.PlugIn.dll")));
        host.SetToggle("PKHeX.Everywhere.Engine.Tests.PlugIn", LevelUp, true);
        Edit(session, PokemonHandle.Party(0));
        Value(Dispatch(session, "pokemon.setLevel", Args(Draft, 41)));
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Value(Dispatch(session, "plugins.run", Args(LevelUp, Draft)));

        changes.Should().ContainSingle().Which.Should().Contain(Topics.All);
        Exported(session, Draft).Pokemon.CurrentLevel.Should().Be(42);
    }

    private static PokemonHandle Slot(Session session, bool inBox) =>
        inBox ? FirstBoxPokemon(session.Game!)!.Value.At : PokemonHandle.Party(0);

    private static int BoxIndex(PKHeX.Facade.Game game, PokemonHandle at) => at.Box!.Value * game.SaveFile.BoxSlotCount + at.Slot;

    private static void Edit(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.edit", Args(at)));

    private static void Update(Session session, PokemonHandle at, object patch) => Value(Dispatch(session, "pokemon.update", Args(at, patch)));

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static (string FileName, PKHeX.Core.PKM Pokemon) Exported(Session session, PokemonHandle at)
    {
        var file = Value(Dispatch(session, "pokemon.export", Args(at)))!;
        var bytes = Convert.FromBase64String(file["bytes"]!.GetValue<string>());
        return (file["fileName"]!.GetValue<string>(), PKHeX.Core.EntityFormat.GetFromBytes(bytes)!);
    }

    private static JsonNode Get(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.get", Args(at)))!;
}
