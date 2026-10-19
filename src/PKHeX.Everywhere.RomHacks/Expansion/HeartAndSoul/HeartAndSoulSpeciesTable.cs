using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Expansion.Imperium;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

// Written from the enum order in Heart & Soul's include/constants/species.h: https://github.com/PokemonHnS-Development/pokehns-expansion, tag Release-v2.0.6.
// Up to 1488 the ids are Imperium's. After them come the Gigantamax forms, which read as their stored form as Imperium's do, and the Legends Z-A Megas.
internal static class HeartAndSoulSpeciesTable
{
    public static readonly CfruSpecies[] NationalByIndex =
    [
        .. ImperiumSpeciesTable.NationalByIndex[..1489],
        (3, 0), (9, 0), (6, 0), (12, 0), (25, 0), (52, 0), (68, 0), (94, 0), (99, 0), (131, 0), (133, 0), (143, 0), (569, 0),
        (809, 0), (812, 0), (815, 0), (818, 0), (823, 0), (826, 0), (834, 0), (839, 0), (841, 0), (842, 0), (844, 0), (849, 0),
        (849, 1), (851, 0), (858, 0), (861, 0), (869, 0), (879, 0), (884, 0), (892, 0), (892, 1), (778, 3),
        (36, 1), (71, 1), (121, 1), (149, 1), (154, 1), (160, 1), (227, 1), (478, 1), (500, 1), (530, 1), (545, 1), (560, 1),
        (604, 1), (609, 1), (652, 1), (655, 1), (658, 3), (668, 1), (687, 1), (691, 1), (701, 1), (670, 6), (689, 1), (718, 5),
        (780, 1), (870, 1), (485, 1), (491, 1), (807, 1), (26, 2), (26, 3), (358, 1), (359, 2), (398, 1), (445, 2), (448, 2),
        (623, 1), (678, 2), (678, 3), (740, 1), (768, 1), (801, 2), (801, 3), (952, 1), (998, 1), (978, 3), (978, 4), (978, 5),
        (970, 1),
    ];
}
