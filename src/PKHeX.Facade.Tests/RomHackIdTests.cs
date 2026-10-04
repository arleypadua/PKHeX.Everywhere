using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;

namespace PKHeX.Facade.Tests;

public class RomHackIdTests
{
    [Fact]
    public void PKHeXsSpeciesMoveAndItemIdsStayFarBelowTheRomHackIdBase()
    {
        int[] counts = [(int)Species.MAX_COUNT, (int)Move.MAX_COUNT, GameInfo.Strings.itemlist.Length];

        counts.Should().OnlyContain(count => count < RomHackIds.Base / 2);
    }

    public static TheoryData<string, int> HackTables => new()
    {
        { "Unbound species", UnboundSpeciesTable.NationalByIndex.Length },
        { "Radical Red species", RadicalRedSpeciesTable.NationalByIndex.Length },
        { "CFRU moves", CfruMoveTable.NationalByIndex.Length },
        { "Unbound items", UnboundItemTable.ModernByIndex.Length },
        { "Radical Red items", RadicalRedItemTable.ModernByIndex.Length },
    };

    [Theory]
    [MemberData(nameof(HackTables))]
    public void EveryHackIndexPlusTheBaseFitsInAUshort(string table, int length) =>
        (RomHackIds.Base + length - 1).Should().BeLessThanOrEqualTo(ushort.MaxValue, table);
}
