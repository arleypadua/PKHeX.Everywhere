using PKHeX.Core;

namespace PKHeX.Facade;

/**
 * Some ROM hacks keep a Gen 3 save container with valid checksums, so PKHeX loads them as the base game. Their
 * Pokémon store species and moves the base game doesn't have, and exporting such a save writes over data the hack owns.
 */
internal static class Gen3Limits
{
    public static bool AreExceededBy(SAV3 save)
    {
        var maxSpeciesInternal = Enumerable.Range(1, save.MaxSpeciesID).Max(species => SpeciesConverter.GetInternal3((ushort)species));
        return Enumerable.Range(0, 6).Select(save.GetPartySlotAtIndex).Concat(save.BoxData).Cast<PK3>().Any(pokemon =>
            pokemon.SpeciesInternal > maxSpeciesInternal ||
            new[] { pokemon.Move1, pokemon.Move2, pokemon.Move3, pokemon.Move4 }.Any(move => move > save.MaxMoveID));
    }
}
