using PKHeX.Everywhere.RomHacks.Expansion.Imperium;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

// Written from the enum order in Heart & Soul's include/constants/items.h: https://github.com/PokemonHnS-Development/pokehns-expansion, tag Release-v2.0.6.
// Up to 681 the ids are Imperium's. The rest map the way Imperium's items of the same name do, and the Legends Z-A Mega Stones to theirs.
// Heart & Soul's own items from 874 map by name to the HeartGold and SoulSilver key items, and the rest read as Unknown.
// Its room keys read as Unknown too, since the table already maps PKHeX's room keys to Emerald's.
internal static class HeartAndSoulItemTable
{
    public static readonly ushort[] ModernByIndex =
    [
        .. ImperiumItemTable.ModernByIndex[..682],
        420, 421, 422, 423, 424, 425, 737, 427, 631, 632, 1267, 1587, 1278, 466, 638, 628, 847, 765, 943, 944, 1590, 696, 933,
        1077, 0, 718, 719, 445, 446, 447, 471, 442, 443, 123, 0, 121, 444, 0, 720, 431, 0, 722, 651, 0, 0, 736, 726, 0, 0, 0, 725,
        721, 728, 735, 723, 727, 734, 730, 731, 732, 733, 2554, 0, 457, 875, 872, 0, 877, 876, 878, 874, 0, 0, 113, 0, 0, 1881,
        1882, 1884, 1885, 1886, 2344, 1880, 1843, 2137, 2345, 1861, 1883, 1857, 1858, 1833, 1842, 1873, 1877, 1876, 1865, 1879,
        1868, 1863, 1871, 1875, 1866, 1870, 1867, 1862, 1869, 1872, 1874, 1878, 1864, 1777, 1779, 1778, 1691, 1611, 1692, 0, 2401,
        2402, 2403, 2404, 2406, 2407, 2408, 2411, 2412, 2413, 2414, 2415, 2416, 2479, 2481, 2482, 2549, 1614, 1630, 1631, 1755,
        1758, 1757, 1756, 1615, 1617, 1619, 1618, 1720, 1785,
        2559, 2560, 2561, 2562, 2563, 2564, 2565, 2566, 2569, 2570, 2571, 2572, 2573, 2574, 2575, 2576, 2577, 2578, 2579, 2580,
        2581, 2582, 2583, 2584, 2585, 2587, 2567, 2568, 2586, 2635, 2636, 2637, 2638, 2639, 2640, 2641, 2642, 2643, 2644, 2645,
        2646, 2647, 2648, 2649, 2650,
        0, 0, 0, 0, 0, 0, 0, 474, 479, 481, 484, 480, 483, 478, 0, 482, 503, 0, 477, 0, 0, 0, 0, 0, 0, 0, 455,
    ];
}
