using PKHeX.Everywhere.RomHacks.Expansion.Imperium;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

// Written from the enum order in Heart & Soul's include/constants/moves.h: https://github.com/PokemonHnS-Development/pokehns-expansion, tag Release-v2.0.6.
// Its moves are Imperium's up to 847. Imperium's own moves after them don't exist in Heart & Soul, whose Z-Moves and Max Moves follow.
internal static class HeartAndSoulMoveTable
{
    public static readonly MoveMap Map = new(ImperiumMoveTable.NationalByIndex[..848]);
}
