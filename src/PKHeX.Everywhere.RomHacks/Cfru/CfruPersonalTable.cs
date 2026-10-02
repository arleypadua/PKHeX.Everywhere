using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// Base stats from the latest game that has each species, with presence decided by the hack's species map.
/// </summary>
public sealed class CfruPersonalTable(CfruSpeciesMap map) : IPersonalTable
{
    private static readonly IPersonalTable[] Sources =
        [PersonalTable.SV, PersonalTable.ZA, PersonalTable.LA, PersonalTable.SWSH, PersonalTable.BDSP, PersonalTable.USUM, PersonalTable.GG];

    public ushort MaxSpeciesID => PersonalTable.SV.MaxSpeciesID;
    public int Count => PersonalTable.SV.Count;
    public PersonalInfo this[int index] => PersonalTable.SV[index];
    public PersonalInfo this[ushort species, byte form] => GetFormEntry(species, form);
    public int GetFormIndex(ushort species, byte form) => PersonalTable.SV.GetFormIndex(species, form);

    public PersonalInfo GetFormEntry(ushort species, byte form) =>
        (Sources.FirstOrDefault(table => table.IsPresentInGame(species, form)) ?? PersonalTable.SV).GetFormEntry(species, form);

    public bool IsSpeciesInGame(ushort species) => map.Contains(species);
    public bool IsPresentInGame(ushort species, byte form) => map.Contains(species, form);
}
