using System.Reflection;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using Xunit.Sdk;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

// Every fixture, official or ROM hack, runs these scenarios through command dispatch.
public class SaveScenarioTests
{
    [Theory]
    [EverySave]
    public void Loads(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var session = LoadSession(saveFile);

        Value(Dispatch(session, "game.get", "[]"))!["fileName"]!.GetValue<string>().Should().Be(Path.GetFileName(saveFile));
        Value(Dispatch(session, "trainer.get", "[]"))!["name"]!.GetValue<string>().Should().NotBeNullOrEmpty();
    });

    [Theory]
    [EverySave]
    public void ListsThePartyAndTheBoxes(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var session = LoadSession(saveFile);

        var party = Value(Dispatch(session, "party.get", "[]"))!.AsArray();
        var boxed = Value(Dispatch(session, "box.get", "[]"))!.AsArray();

        party.Should().NotBeEmpty();
        boxed.Should().HaveCount(BoxedPokemon(session.Game!).Count());
        party.Select(p => p!["at"]!["slot"]!.GetValue<int>()).Should().Equal(Enumerable.Range(0, party.Count));
        party.Concat(boxed).Select(p => p!["species"]!.GetValue<string>()).Should().NotContain("");
        party.Concat(boxed).Select(p => p!["level"]!.GetValue<int>()).Should().OnlyContain(level => level >= 1 && level <= 100);
    });

    [Theory]
    [EverySave]
    public void EveryListedPokemonHasASpeciesOrIsUnknown(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
        Listed(LoadSession(saveFile)).Should().OnlyContain(p => p["isUnknown"]!.GetValue<bool>()
            ? p["speciesId"] == null && !p["editable"]!.GetValue<bool>()
            : p["speciesId"]!.GetValue<int>() != 0 && p["editable"]!.GetValue<bool>()));

    [Theory]
    [EverySave]
    public void EveryListedPokemonOpensInTheEditorOrReadOnly(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var session = LoadSession(saveFile);

        foreach (var pokemon in Listed(session))
        {
            if (pokemon["editable"]!.GetValue<bool>())
            {
                Value(Dispatch(session, "pokemon.edit", Args(Handle(pokemon))));
                Value(Dispatch(session, "pokemon.details", Args(PokemonHandle.Draft())));
            }
            else
            {
                Value(Dispatch(session, "pokemon.details", Args(Handle(pokemon))))!["editable"]!.GetValue<bool>().Should().BeFalse();
                Error(Dispatch(session, "pokemon.edit", Args(Handle(pokemon)))).Should().Be("unknown-species");
            }
        }
    });

    [Theory]
    [EverySave]
    [KnownGap(SaveFilePath.Yellow, "A box Pokémon's id hashes its party level, which the box doesn't store, so the id changes on export.")]
    public void AnEditedPokemonIsCommittedAndSurvivesExport(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var session = LoadSession(saveFile);
        var edited = new[] { PokemonHandle.Party(0), FirstBoxPokemon(session.Game!)!.Value.At };

        foreach (var at in edited)
        {
            var level = Details(session, at)["level"]!.GetValue<int>() == 50 ? 51 : 50;
            Value(Dispatch(session, "pokemon.edit", Args(at)));
            Value(Dispatch(session, "pokemon.update", Args(PokemonHandle.Draft(), new { nickname = "Edited", level })));
            Value(Dispatch(session, "pokemon.commit", "[]"));

            Details(session, at)["nickname"]!.GetValue<string>().Should().Be("Edited");
            Details(session, at)["level"]!.GetValue<int>().Should().Be(level);
        }

        var reloaded = LoadSession(saveFile, Exported(session));
        foreach (var at in edited) Details(reloaded, at).ToJsonString().Should().Be(Details(session, at).ToJsonString());
        ListedJson(reloaded).Should().Equal(ListedJson(session));
    });

    // The Engine has no command to move a Pokémon, so two swap places in the save before it loads, as a move in the game would.
    // Let's Go Eevee has no Pokémon outside its party, so it swaps two party members.
    [Theory]
    [EverySave]
    public void APokemonMovedToAnotherSlotSurvivesExport(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var game = SaveFilePath.Load(saveFile);
        var boxed = BoxedPokemon(game).Take(2).Select(p => p.At).ToArray();
        var (first, second) = boxed.Length == 2 ? (boxed[0], boxed[1]) : (PokemonHandle.Party(0), PokemonHandle.Party(1));
        var before = LoadSession(saveFile);
        var (pokemonA, pokemonB) = (Slot(game, first), Slot(game, second));
        SetSlot(game, first, pokemonB);
        SetSlot(game, second, pokemonA);

        var moved = LoadSession(saveFile, game.SaveFile.Write().ToArray());
        var reloaded = LoadSession(saveFile, Exported(moved));

        foreach (var session in new[] { moved, reloaded })
        {
            Summary(session, second).Should().Be(Summary(before, first));
            Summary(session, first).Should().Be(Summary(before, second));
        }
    });

    [Theory]
    [EverySave]
    public void SettingAndRemovingBagItemsSurvivesExport(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var session = LoadSession(saveFile);
        var addable = AddableItem(session.Game!);
        addable.Should().NotBeNull("the bag should have room for an item");
        var (added, maxCount) = addable!.Value;
        var removed = OwnedItem(session.Game!)!;

        Value(Dispatch(session, "inventory.setItem", Args(added, maxCount)));
        Value(Dispatch(session, "inventory.setItem", Args(removed, 0)));

        var reloaded = LoadSession(saveFile, Exported(session));
        Owned(reloaded, added.Pouch).Should().Contain((added.ItemId, maxCount));
        Owned(reloaded, removed.Pouch).Select(item => item.Id).Should().NotContain(removed.ItemId);
        Value(Dispatch(reloaded, "inventory.get", "[]"))!.ToJsonString()
            .Should().Be(Value(Dispatch(session, "inventory.get", "[]"))!.ToJsonString());
    });

    [Theory]
    [EverySave]
    [KnownGap(SaveFilePath.Crystal, "The key items pocket lists item 255, which it can't set.")]
    [KnownGap(SaveFilePath.HgSs, "The Berries pocket lists a Max Repel with a count of 0, which it can't set.")]
    public void EveryListedBagItemCanBeChangedAndRemoved(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var session = LoadSession(saveFile);

        foreach (var pouch in Value(Dispatch(session, "inventory.get", "[]"))!.AsArray())
        foreach (var item in pouch!["items"]!.AsArray())
        {
            var at = new ItemHandle(pouch["name"]!.GetValue<string>(), item!["id"]!.GetValue<int>());
            Value(Dispatch(session, "inventory.setItem", Args(at, 1)));
            Value(Dispatch(session, "inventory.setItem", Args(at, 0)));
        }

        Value(Dispatch(LoadSession(saveFile, Exported(session)), "inventory.get", "[]"))!.AsArray()
            .Should().OnlyContain(pouch => pouch!["items"]!.AsArray().Count == 0);
    });

    [Theory]
    [EverySave]
    public void EveryOfferedNatureCanBeStored(string saveFile, string? knownGap) =>
        PassesUnlessKnownGap(knownGap, () => EveryChoiceIsStored(saveFile, "nature", session => Value(Dispatch(session, "game.natures", "[]"))));

    [Theory]
    [EverySave]
    public void EveryOfferedBallCanBeStored(string saveFile, string? knownGap) =>
        PassesUnlessKnownGap(knownGap, () => EveryChoiceIsStored(saveFile, "ball", session => Value(Dispatch(session, "game.balls", "[]"))));

    [Theory]
    [EverySave]
    public void EveryOfferedLanguageCanBeStored(string saveFile, string? knownGap) =>
        PassesUnlessKnownGap(knownGap, () => EveryChoiceIsStored(saveFile, "language", session => Value(Dispatch(session, "game.languages", "[]"))));

    [Theory]
    [EverySave]
    public void EveryOfferedOriginGameCanBeStored(string saveFile, string? knownGap) =>
        PassesUnlessKnownGap(knownGap, () => EveryChoiceIsStored(saveFile, "version", session => Value(Dispatch(session, "game.originGames", "[]"))));

    [Theory]
    [EverySave]
    public void EveryOfferedAbilityCanBeStored(string saveFile, string? knownGap) =>
        PassesUnlessKnownGap(knownGap, () => EveryChoiceIsStored(saveFile, "ability", session => Options(session)["abilities"]));

    [Theory]
    [EverySave]
    public void EveryOfferedMetLocationCanBeStored(string saveFile, string? knownGap) =>
        PassesUnlessKnownGap(knownGap, () => EveryChoiceIsStored(saveFile, "metLocation", session => Options(session)["metLocations"]));

    // PKHeX rewrites the checksums and backups of some saves on any write, so for those the bytes to keep are the ones its writer gives.
    [Theory]
    [EverySave]
    public void ExportingWithoutEditsReturnsTheSameBytes(string saveFile, string? knownGap) => PassesUnlessKnownGap(knownGap, () =>
    {
        var save = SaveFilePath.Load(saveFile).SaveFile;
        var expected = RewrittenByPKHeX.Contains(saveFile)
            ? save.Write(save.Metadata.GetSuggestedFlags(Path.GetExtension(saveFile))).ToArray()
            : File.ReadAllBytes(saveFile);

        Exported(LoadSession(saveFile)).Should().Equal(expected);
    });

    private static readonly string[] RewrittenByPKHeX = [SaveFilePath.Yellow, SaveFilePath.Crystal, SaveFilePath.FireRed, SaveFilePath.HgSs];

    private static void PassesUnlessKnownGap(string? knownGap, Action scenario)
    {
        if (knownGap is null)
        {
            scenario();
            return;
        }

        scenario.Should().Throw<XunitException>($"the save has a known gap: {knownGap} If it now passes, remove the [KnownGap]");
    }

    private static Session LoadSession(string saveFile, byte[]? bytes = null)
    {
        var session = new Session();
        var args = Args(Convert.ToBase64String(bytes ?? File.ReadAllBytes(saveFile)), Path.GetFileName(saveFile), SaveFilePath.FormatOf(saveFile)?.Id!);
        Value(Dispatch(session, "game.load", args));
        return session;
    }

    private static byte[] Exported(Session session) =>
        Convert.FromBase64String(Value(Dispatch(session, "game.export", "[]"))!["bytes"]!.GetValue<string>());

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static string Summary(Session session, PokemonHandle at)
    {
        var summary = Value(Dispatch(session, "pokemon.get", Args(at)))!.AsObject();
        summary.Remove("at");
        return summary.ToJsonString();
    }

    private static IEnumerable<JsonNode> Listed(Session session) =>
        Value(Dispatch(session, "party.get", "[]"))!.AsArray().Concat(Value(Dispatch(session, "box.get", "[]"))!.AsArray())!;

    private static string[] ListedJson(Session session) => Listed(session).Select(p => p.ToJsonString()).ToArray();

    private static PokemonHandle Handle(JsonNode pokemon)
    {
        var at = pokemon["at"]!;
        var slot = at["slot"]!.GetValue<int>();
        return at["box"] is { } box ? PokemonHandle.InBox(box.GetValue<int>(), slot) : PokemonHandle.Party(slot);
    }

    private static PKM Slot(Facade.Game game, PokemonHandle at) => at.Box is { } box
        ? game.SaveFile.GetBoxSlotAtIndex(box, at.Slot)
        : game.SaveFile.GetPartySlotAtIndex(at.Slot);

    private static void SetSlot(Facade.Game game, PokemonHandle at, PKM pokemon)
    {
        if (at.Box is { } box) game.SaveFile.SetBoxSlotAtIndex(pokemon, box, at.Slot);
        else game.SaveFile.SetPartySlotAtIndex(pokemon, at.Slot);
    }

    private static (int Id, int Count)[] Owned(Session session, string pouch) => Value(Dispatch(session, "inventory.get", "[]"))!.AsArray()
        .Single(p => p!["name"]!.GetValue<string>() == pouch)!["items"]!.AsArray()
        .Select(item => (item!["id"]!.GetValue<int>(), item["count"]!.GetValue<int>()))
        .ToArray();

    private static JsonNode Options(Session session) => Value(Dispatch(session, "pokemon.options", Args(PokemonHandle.Party(0))))!;

    private static void EveryChoiceIsStored(string saveFile, string field, Func<Session, JsonNode?> choices)
    {
        var session = LoadSession(saveFile);
        if (Options(session)["locked"]!.AsArray().Any(locked => locked!.GetValue<string>() == field)) return;

        var unstorable = choices(session)!.AsArray()
            .Select(choice => choice!["id"]!.GetValue<int>())
            .Where(id => !Stores(session, field, id))
            .ToArray();

        unstorable.Should().BeEmpty($"every {field} offered should be stored");
    }

    private static bool Stores(Session session, string field, int id)
    {
        Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.Party(0))));
        var update = JsonNode.Parse(Dispatch(session, "pokemon.update", Args(PokemonHandle.Draft(), new Dictionary<string, int> { [field] = id })))!;
        return update["ok"]!.GetValue<bool>() && Details(session, PokemonHandle.Draft())[field]!.GetValue<int>() == id;
    }
}

public class EverySaveAttribute : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var gaps = testMethod.GetCustomAttributes<KnownGapAttribute>().ToDictionary(gap => gap.SaveFile, gap => gap.Reason);
        return SaveFilePath.All.Append(SaveFilePath.UnboundUnknownSpecies)
            .Select(saveFile => new object[] { saveFile, gaps.GetValueOrDefault(saveFile)! });
    }
}

// A gap the save has today. The scenario must fail on that save, so fixing the gap fails the test until the attribute goes.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class KnownGapAttribute(string saveFile, string reason) : Attribute
{
    public string SaveFile { get; } = saveFile;
    public string Reason { get; } = reason;
}
