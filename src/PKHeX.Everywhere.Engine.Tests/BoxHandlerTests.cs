using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using PokemonFile = PKHeX.Facade.Tests.Base.PokemonFile;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

// Files are converted and judged under PKHeX settings that are global, so no other test runs meanwhile.
[Collection(nameof(TransferHandlerTests))]
public class BoxHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void GetReturnsEveryBoxedPokemonInBoxAndSlotOrder(string saveFile)
    {
        var session = Loaded(saveFile);
        var game = session.Game!;

        var box = Value(Dispatch(session, "box.get", "[]"))!.AsArray();

        var expected = BoxedPokemon(game).ToList();
        box.Select(p => p!["id"]!.GetValue<string>()).Should().Equal(expected.Select(p => p.Pokemon.UniqueId.Value));
        box.Select(p => p!["at"]!.ToJsonString()).Should().Equal(expected.Select(p => Args(p.At)[1..^1]));
        box.Select(p => p!["species"]!.GetValue<string>()).Should().Equal(expected.Select(p => p.Pokemon.Species.Name));
    }

    [Fact]
    public void ListReturnsEveryYellowBoxWithItsSlotCount()
    {
        var boxes = Value(Dispatch(Loaded(SaveFilePath.Yellow), "box.list", "[]"))!.AsArray();

        boxes.Select(b => b!.ToJsonString()).Should().Equal(Enumerable.Range(0, 12).Select(box => $$"""{"box":{{box}},"name":null,"slots":20}"""));
    }

    [Fact]
    public void ListReturnsTheBoxNamesTheSaveStores()
    {
        var boxes = Value(Dispatch(Loaded(SaveFilePath.FireRed), "box.list", "[]"))!.AsArray();

        boxes.Select(b => b!["name"]!.GetValue<string>()).Should().StartWith(["10A EUR1", "10A EUR2"]);
    }

    [Theory]
    [SupportedSaveFiles]
    public void EveryBoxHandleFromGetIsInAListedBox(string saveFile)
    {
        var session = Loaded(saveFile);
        var slots = Value(Dispatch(session, "box.list", "[]"))!.AsArray()
            .ToDictionary(b => b!["box"]!.GetValue<int>(), b => b!["slots"]!.GetValue<int>());

        Value(Dispatch(session, "box.get", "[]"))!.AsArray().Should().AllSatisfy(p =>
        {
            var box = p!["at"]!["box"]!.GetValue<int>();
            slots.Should().ContainKey(box);
            p["at"]!["slot"]!.GetValue<int>().Should().BeLessThan(slots[box]);
        });
    }

    [Fact]
    public void GetFailsWithNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "box.get", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.Unbound21, SaveFilePath.RadicalRed, SaveFilePath.Imperium])] // a hack Pokémon's file is in the hack's format, which PKHeX can't read back
    public void AddFromFileAddsAPokemonExportedFromTheSaveToTheFirstEmptyBoxSlot(string saveFile)
    {
        var session = Loaded(saveFile);
        var exported = session.Game!.Trainer.Party.Pokemons[0];

        AddsToTheFirstEmptyBoxSlot(session, exported.ToFile().Bytes, exported.Species.Name);
    }

    [Fact]
    public void AddFromFileAddsAPokemonFileToTheFirstEmptyBoxSlot() =>
        AddsToTheFirstEmptyBoxSlot(Loaded(SaveFilePath.LetsGoPikachu), File.ReadAllBytes(PokemonFile.Venusaur), "Venusaur");

    [Fact]
    public void AddFromFileFailsWithUnparseableWhenTheBytesAreNotAPokemon() =>
        FailsWith(Loaded(SaveFilePath.Emerald), [1, 2, 3], "unparseable");

    [Fact]
    public void AddFromFileFailsWithConversionFailedWhenThePokemonCannotMoveToTheSaveFormat() =>
        FailsWith(Loaded(SaveFilePath.HgSs), File.ReadAllBytes(PokemonFile.Golduck), "conversion-failed");

    [Fact]
    public void AddFromFileFailsWithNotInGameWhenTheGameDoesNotHaveTheSpecies()
    {
        var chikorita = new PB7 { Species = (ushort)Species.Chikorita, Version = GameVersion.GP };
        chikorita.RefreshChecksum();
        var bytes = new byte[chikorita.SIZE_PARTY];
        chikorita.WriteDecryptedDataParty(bytes);

        FailsWith(Loaded(SaveFilePath.LetsGoPikachu), bytes, "not-in-game");
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.Unbound21, SaveFilePath.RadicalRed, SaveFilePath.Imperium])] // a hack Pokémon's file is in the hack's format, which PKHeX can't read back
    public void AddFromFileFailsWithBoxFullWhenNoBoxSlotIsEmpty(string saveFile)
    {
        var session = Loaded(saveFile);
        var save = session.Game!.SaveFile;
        var exported = session.Game.Trainer.Party.Pokemons[0];
        for (var index = save.NextOpenBoxSlot(); index >= 0; index = save.NextOpenBoxSlot())
            save.SetBoxSlotAtIndex(exported.Pkm.Clone(), index);

        FailsWith(session, exported.ToFile().Bytes, "box-full");
    }

    [Fact]
    public void AddFromFileFailsWithNoSaveWithoutALoadedSave() =>
        FailsWith(new Session(), File.ReadAllBytes(PokemonFile.Golduck), "no-save");

    [Fact]
    public void PreviewFileShowsAPokemonNoGameCanMoveAsUnofficialAndWritesNothing()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var before = session.Game!.ToByteArray();

        var preview = Value(Dispatch(session, "box.previewFile", Args(Convert.ToBase64String(Gengar4()))))!;

        preview["unofficial"]!.GetValue<bool>().Should().BeTrue();
        preview["pokemon"]!["species"]!.GetValue<string>().Should().Be("Gengar");
        preview["changes"]!.AsArray().Select(c => c!.ToJsonString())
            .Should().Contain("""{"field":"moves","before":"Shadow Claw","after":null,"reason":"notInGame"}""");
        preview["legality"]!["valid"]!.GetValue<bool>().Should().BeFalse();
        session.Game.ToByteArray().Should().Equal(before);
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    [Fact]
    public void PreviewFileOfAPokemonAGameCanMoveIsntUnofficial()
    {
        var bytes = Loaded(SaveFilePath.Emerald).Game!.Trainer.Party.Pokemons[0].ToFile().Bytes;

        Value(Dispatch(Loaded(SaveFilePath.HgSs), "box.previewFile", Args(Convert.ToBase64String(bytes))))!["unofficial"]!.GetValue<bool>().Should().BeFalse();
    }

    [Fact]
    public void PreviewFileFailsLikeAddFromFile()
    {
        var lucario = Pokemon4(new PK4 { Species = (ushort)Species.Lucario, CurrentLevel = 50, Move1 = (ushort)Move.AuraSphere });

        Error(Dispatch(Loaded(SaveFilePath.Emerald), "box.previewFile", Args(Convert.ToBase64String(new byte[] { 1, 2, 3 })))).Should().Be("unparseable");
        Error(Dispatch(Loaded(SaveFilePath.Emerald), "box.previewFile", Args(Convert.ToBase64String(lucario)))).Should().Be("not-in-game");
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    [Fact]
    public void AddFromFileFailsWithConversionFailedForAnUnofficialPokemonUnlessAllowed()
    {
        FailsWith(Loaded(SaveFilePath.Emerald), Gengar4(), "conversion-failed");

        var session = Loaded(SaveFilePath.Emerald);
        var added = Value(Dispatch(session, "box.addFromFile", Args(Convert.ToBase64String(Gengar4()), new { allowUnofficial = true })))!;

        var arrived = session.Game!.Trainer.PokemonBox.All.Single(p => p.UniqueId.Value == added["id"]!.GetValue<string>());
        arrived.Species.Name.Should().Be("Gengar");
        arrived.Pkm.Should().BeOfType<PK3>();
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    [Fact]
    public void AddFromFileRestoresTheConversionSettingWhenItFails()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var save = session.Game!.SaveFile;
        for (var index = save.NextOpenBoxSlot(); index >= 0; index = save.NextOpenBoxSlot())
            save.SetBoxSlotAtIndex(save.GetPartySlotAtIndex(0), index);

        Error(Dispatch(session, "box.addFromFile", Args(Convert.ToBase64String(Gengar4()), new { allowUnofficial = true }))).Should().Be("box-full");
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    private static byte[] Gengar4() =>
        Pokemon4(new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowClaw, Move2 = (ushort)Move.ShadowBall });

    private static byte[] Pokemon4(PK4 pokemon)
    {
        pokemon.RefreshChecksum();
        var bytes = new byte[pokemon.SIZE_PARTY];
        pokemon.WriteDecryptedDataParty(bytes);
        return bytes;
    }

    private static void AddsToTheFirstEmptyBoxSlot(Session session, byte[] bytes, string species)
    {
        var game = session.Game!;
        var index = game.SaveFile.NextOpenBoxSlot();
        var slots = game.SaveFile.BoxSlotCount;
        var expected = PokemonHandle.InBox(index / slots, index % slots);
        var changes = new List<string[]>();
        var published = new List<IEngineEvent>();
        session.Changed += changes.Add;
        session.Published += published.Add;

        var added = Value(Dispatch(session, "box.addFromFile", Args(Convert.ToBase64String(bytes), null!)))!;

        added["at"]!.ToJsonString().Should().Be(Args(expected)[1..^1]);
        var pokemon = Value(Dispatch(session, "pokemon.get", Args(expected)))!;
        pokemon["species"]!.GetValue<string>().Should().Be(species);
        added["id"]!.GetValue<string>().Should().Be(pokemon["id"]!.GetValue<string>());
        changes.Should().ContainSingle().Which.Should().Equal(Topics.Box);
        published.Should().Equal(new PokemonAdded(expected, PokemonAddSource.File, game.Trainer.PokemonBox.All[index].ToOverview()));
    }

    private static void FailsWith(Session session, byte[] bytes, string code)
    {
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Error(Dispatch(session, "box.addFromFile", Args(Convert.ToBase64String(bytes), null!))).Should().Be(code);

        published.Should().BeEmpty();
    }
}
