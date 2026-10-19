using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class HeartAndSoulTrainerTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.HeartAndSoul);

    private const uint EncryptionKey = 0xC2F8B864;

    // The fixture's first slot has counter 100 and its second 101.
    private const int NewerSlot = 14;

    private static Game Load(byte[]? bytes = null) => Game.LoadFrom(bytes ?? Fixture.ToArray(), SaveFilePath.HeartAndSoul);

    private static int SectorOf(int id) =>
        Enumerable.Range(NewerSlot, 14).Single(sector => ReadUInt16LittleEndian(Fixture.AsSpan((sector * 0x1000) + 0xFF4)) == id) * 0x1000;

    private static int SaveBlock2 => SectorOf(0);
    private static int Money => SectorOf(1) + 0x494;

    [Fact]
    public void ReadsTheTrainer()
    {
        var game = Load();

        game.Trainer.Name.Should().Be("Pukster");
        game.Trainer.Gender.Should().Be(Gender.Male);
        game.Trainer.Id.Should().Be(new EntityId(18233, 57075));
        game.Trainer.Money.IsSupported.Should().BeTrue();
        game.Trainer.Money.Amount.Should().Be(1026739);
    }

    [Fact]
    public void MoneyIsCappedAt9999999()
    {
        var game = Load();

        game.Trainer.Money.SetMax();

        game.Trainer.Money.Amount.Should().Be(9999999);
    }

    [Fact]
    public void EditsSurviveExportAndChangeNoOtherByte()
    {
        var game = Load();

        game.Trainer.Name = "Gold";
        game.Trainer.Gender = Gender.Female;
        game.SaveFile.TID16 = 12345;
        game.SaveFile.SID16 = 54321;
        game.Trainer.Money.Set(424242);

        var exported = game.ToByteArray();
        ReadUInt32LittleEndian(exported.AsSpan(Money)).Should().Be(424242 ^ EncryptionKey);
        var owned = new HashSet<int>(Enumerable.Range(NewerSlot, 14).SelectMany(sector => new[] { (sector * 0x1000) + 0xFF6, (sector * 0x1000) + 0xFF7 }));
        owned.UnionWith(Enumerable.Range(SaveBlock2, 9));
        owned.UnionWith(Enumerable.Range(SaveBlock2 + 0x0A, 4));
        owned.UnionWith(Enumerable.Range(Money, 4));
        Enumerable.Range(0, Fixture.Length).Where(index => exported[index] != Fixture[index])
            .Should().NotBeEmpty().And.OnlyContain(index => owned.Contains(index));

        var reloaded = Load(exported);
        reloaded.SaveFile.ChecksumsValid.Should().BeTrue();
        reloaded.Trainer.Name.Should().Be("Gold");
        reloaded.Trainer.Gender.Should().Be(Gender.Female);
        reloaded.Trainer.Id.Should().Be(new EntityId(12345, 54321));
        reloaded.Trainer.Money.Amount.Should().Be(424242);
    }
}
