using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class ImperiumTrainerTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Imperium);

    private const uint EncryptionKey = 0x96BEE858;

    private static Game Load(byte[]? bytes = null) => Game.LoadFrom(bytes ?? Fixture.ToArray(), SaveFilePath.Imperium);

    private static int SectorOf(int id) =>
        Enumerable.Range(0, 28).Single(sector => ReadUInt16LittleEndian(Fixture.AsSpan((sector * 0x1000) + 0xFF4)) == id) * 0x1000;

    private static int SaveBlock2 => SectorOf(0);
    private static int Money => SectorOf(1) + 0x490;

    [Fact]
    public void ReadsTheTrainer()
    {
        var trainer = Load().Trainer;

        trainer.Name.Should().Be("A");
        trainer.Gender.Should().Be(Gender.Male);
        trainer.Id.Should().Be(new EntityId(27859, 47663));
        trainer.Money.IsSupported.Should().BeTrue();
        trainer.Money.Amount.Should().Be(119322);
    }

    [Fact]
    public void MoneyIsCappedAt999999()
    {
        var game = Load();

        game.Trainer.Money.SetMax();

        game.Trainer.Money.Amount.Should().Be(999999);
    }

    [Fact]
    public void AnEditedNameAndMoneySurviveExportAndChangeNoOtherByte()
    {
        var game = Load();

        game.Trainer.Name = "Brendan";
        game.Trainer.Money.Set(424242);

        var exported = game.ToByteArray();
        ReadUInt32LittleEndian(exported.AsSpan(Money)).Should().Be(424242 ^ EncryptionKey);
        var owned = new HashSet<int>(Enumerable.Range(0, 28).SelectMany(sector => new[] { (sector * 0x1000) + 0xFF6, (sector * 0x1000) + 0xFF7 }));
        owned.UnionWith(Enumerable.Range(SaveBlock2, 7));
        owned.UnionWith(Enumerable.Range(Money, 4));
        Enumerable.Range(0, Fixture.Length).Where(index => exported[index] != Fixture[index])
            .Should().NotBeEmpty().And.OnlyContain(index => owned.Contains(index));

        var reloaded = Load(exported);
        reloaded.SaveFile.ChecksumsValid.Should().BeTrue();
        reloaded.Trainer.Name.Should().Be("Brendan");
        reloaded.Trainer.Money.Amount.Should().Be(424242);
    }
}
