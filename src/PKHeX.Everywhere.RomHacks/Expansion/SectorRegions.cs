namespace PKHeX.Everywhere.RomHacks.Expansion;

internal static class SectorRegions
{
    // A save block runs on across consecutive sectors, so a run of slots can start in one sector and end in the next.
    public static (int Offset, int Slots)[] Of(ReadOnlySpan<int> blockSectors, int sectorData, int offset, int slots, int slotSize)
    {
        List<(int Offset, int Slots)> regions = [];
        while (slots > 0)
        {
            var inSector = offset % sectorData;
            var fit = Math.Min(slots, (sectorData - inSector) / slotSize);
            regions.Add((blockSectors[offset / sectorData] + inSector, fit));
            offset += fit * slotSize;
            slots -= fit;
        }

        return [..regions];
    }
}
