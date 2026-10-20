using AwesomeAssertions;
using PKHeX.Core;
using static PKHeX.Facade.Tests.Base.GameCubeSaves;

namespace PKHeX.Facade.Tests;

public class MemoryCardTests
{
    [Theory]
    [InlineData(ColosseumCode, typeof(SAV3Colosseum))]
    [InlineData(XDCode, typeof(SAV3XD))]
    [InlineData(BoxCode, typeof(SAV3RSBox))]
    public void LoadsTheSaveOnACard(string gameCode, Type saveType)
    {
        var card = new MemoryCardBuilder().Add(OtherGameCode, new byte[MemoryCardBuilder.BlockSize]).Add(gameCode, File(gameCode)).Build();

        var game = Game.LoadFrom(card.Bytes, "card.raw");

        game.SaveFile.Should().BeOfType(saveType);
        game.Trainer.PokemonBox.All.Should().Contain(pokemon => pokemon.Nickname == StoredNickname);
    }

    [Theory]
    [InlineData(ColosseumCode)]
    [InlineData(XDCode)]
    [InlineData(BoxCode)]
    public void ExportWritesOnlyTheEditedFileBackIntoTheCard(string gameCode)
    {
        var card = new MemoryCardBuilder().AddAroundAnotherFile(gameCode, File(gameCode)).Build();
        var game = Game.LoadFrom(card.Bytes, "card.raw");

        game.Trainer.PokemonBox.All.Single(pokemon => pokemon.Nickname == StoredNickname).ChangeNickname(EditedNickname);
        var exported = game.ToByteArray();

        MemoryCardBuilder.ChecksumsPass(exported).Should().BeTrue();
        ChangedBytes(card.Bytes, exported).Should().OnlyContain(at => IsInFile(card, gameCode, at) || IsItsEntryOrDirectoryChecksum(card, gameCode, at));
        FileOf(exported, card, gameCode)[..0x40].Should().Equal(FileOf(card.Bytes, card, gameCode)[..0x40]);
        Game.LoadFrom(exported, "card.raw").Trainer.PokemonBox.All.Should().Contain(pokemon => pokemon.Nickname == EditedNickname);
    }

    public static TheoryData<string, int[], int> SlotsByCounter => new()
    {
        { ColosseumCode, [5, 9, 7], 1 },
        { XDCode, [9, 5], 0 },
    };

    [Theory]
    [MemberData(nameof(SlotsByCounter))]
    public void ExportKeepsTheEditInTheSlotTheGameLoads(string gameCode, int[] counters, int latest)
    {
        var file = gameCode == ColosseumCode ? ColosseumFile(counters) : XDFile(counters);
        var card = new MemoryCardBuilder().Add(gameCode, file).Build();
        var game = Game.LoadFrom(card.Bytes, "card.raw");

        game.Trainer.PokemonBox.All.Single(pokemon => pokemon.Nickname == StoredNickname).ChangeNickname(EditedNickname);
        var before = FileOf(card.Bytes, card, gameCode);
        var after = FileOf(game.ToByteArray(), card, gameCode);

        var slots = Enumerable.Range(0, counters.Length).ToArray();
        slots.Select(slot => SaveCounter(Slot(gameCode, after, slot))).Should().Equal(counters);
        slots.Where(slot => !Slot(gameCode, before, slot).SequenceEqual(Slot(gameCode, after, slot))).Should().Equal(latest);
    }

    [Fact]
    public void ACardWithSeveralGamesAsksWhichToLoad()
    {
        var card = new MemoryCardBuilder().Add(ColosseumCode, File(ColosseumCode)).Add(XDCode, File(XDCode)).Build();

        var load = () => Game.LoadFrom(card.Bytes, "card.raw");

        load.Should().Throw<FormatChoiceRequiredException>()
            .Which.Candidates.Select(format => format.Id).Should().Equal("memory-card-colosseum", "memory-card-xd");
    }

    [Fact]
    public void ChoosingAGameOnACardLoadsItAndWritesItBack()
    {
        var card = new MemoryCardBuilder().Add(ColosseumCode, File(ColosseumCode)).Add(XDCode, File(XDCode)).Build();
        var choice = Record.Exception(() => Game.LoadFrom(card.Bytes, "card.raw")).As<FormatChoiceRequiredException>()
            .Candidates.Single(format => format.Id == "memory-card-xd");

        var game = Game.LoadFrom(card.Bytes, "card.raw", SaveFormats.Find(choice.Id));
        game.Trainer.PokemonBox.All.Single(pokemon => pokemon.Nickname == StoredNickname).ChangeNickname(EditedNickname);
        var exported = game.ToByteArray();

        game.SaveFile.Should().BeOfType<SAV3XD>();
        ChangedBytes(card.Bytes, exported).Should().OnlyContain(at => IsInFile(card, XDCode, at) || IsItsEntryOrDirectoryChecksum(card, XDCode, at));
        Game.LoadFrom(exported, "card.raw", choice).Trainer.PokemonBox.All.Should().Contain(pokemon => pokemon.Nickname == EditedNickname);
    }

    [Fact]
    public void ACardWithNoPokemonGameFailsToLoad()
    {
        var card = new MemoryCardBuilder().Add(OtherGameCode, new byte[MemoryCardBuilder.BlockSize]).Build();

        var load = () => Game.LoadFrom(card.Bytes, "card.raw");

        load.Should().Throw<GameNotLoadedException>();
    }

    public static TheoryData<string, string> UnsavedFiles => new()
    {
        { ColosseumCode, "card" }, { ColosseumCode, "gci" }, { ColosseumCode, "blocks" },
        { XDCode, "card" }, { XDCode, "gci" }, { XDCode, "blocks" },
    };

    [Theory]
    [MemberData(nameof(UnsavedFiles))]
    public void AFileWithNoStorySavedFailsToLoad(string gameCode, string container)
    {
        var file = gameCode == ColosseumCode ? UnsavedColosseumFile() : UnsavedXDFile();
        var bytes = container switch
        {
            "card" => new MemoryCardBuilder().Add(gameCode, file).Build().Bytes,
            "gci" => Gci(gameCode, file),
            _ => file,
        };

        var load = () => Game.LoadFrom(bytes, "save");

        load.Should().Throw<GameNotLoadedException>();
    }

    [Theory]
    [InlineData(ColosseumCode)]
    [InlineData(XDCode)]
    public void AGciWithAStorySavedLoads(string gameCode) =>
        Game.LoadFrom(Gci(gameCode, File(gameCode)), "save.gci").Trainer.PokemonBox.All.Should().Contain(pokemon => pokemon.Nickname == StoredNickname);

    private const string EditedNickname = "EDITED";

    private static byte[] Slot(string gameCode, byte[] file, int slot) =>
        (gameCode == ColosseumCode ? ColoCrypto.GetSlot(file, slot) : XDCrypto.GetSlot(file, slot)).ToArray();

    private static int SaveCounter(byte[] slot) => System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(slot.AsSpan(4));

    private static IEnumerable<int> ChangedBytes(byte[] before, byte[] after) =>
        Enumerable.Range(0, before.Length).Where(at => before[at] != after[at]);

    private static bool IsInFile(BuiltCard card, string gameCode, int at) =>
        card.Blocks[gameCode].Contains((ushort)(at / MemoryCardBuilder.BlockSize));

    private static bool IsItsEntryOrDirectoryChecksum(BuiltCard card, string gameCode, int at)
    {
        var directory = BuiltCard.ActiveDirectoryBlock * MemoryCardBuilder.BlockSize;
        var entry = directory + (card.Entries[gameCode] * 0x40);
        return at - entry is >= 0 and < 0x40 || at - directory is >= 0x1FFC and < 0x2000;
    }

    private static byte[] FileOf(byte[] bytes, BuiltCard card, string gameCode) => card.Blocks[gameCode]
        .SelectMany(block => bytes.AsSpan(block * MemoryCardBuilder.BlockSize, MemoryCardBuilder.BlockSize).ToArray())
        .ToArray();
}
