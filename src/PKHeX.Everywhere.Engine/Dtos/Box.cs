using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// A box in the save, as <c>box.list</c> lists it. Empty boxes are included.
/// </summary>
/// <param name="Box">Zero-based box number, the same number a box <see cref="PokemonHandle"/> carries.</param>
/// <param name="Name">The name the save stores for the box, or null when the save doesn't store box names, such as Generation 1 saves, Let's Go and ROM hacks.</param>
/// <param name="Slots">How many slots the box has.</param>
public record BoxEntry(int Box, string? Name, int Slots);

public static class BoxMapping
{
    public static BoxEntry ToEntry(this Box box) => new(box.Number, box.Name, box.Slots);
}

/// <summary>
/// A <see cref="PokemonSummary"/> without <c>id</c> and <c>at</c>, for a Pokémon that isn't in a slot.
/// </summary>
/// <param name="SpeciesId">PKHeX species id, which is the National Pokédex number. Null when <c>isUnknown</c> is true.</param>
/// <param name="SpeciesIndex">The species as the save stores it: the game's internal index in Gen 1 to 3, Gen 9 and ROM hacks, and the National Pokédex number elsewhere.</param>
/// <param name="Species">Species name, or a name like <c>Unknown (#412)</c> when <c>isUnknown</c> is true.</param>
/// <param name="IsUnknown">Neither PKHeX nor the save knows the species. Show a placeholder instead of a sprite.</param>
/// <param name="Editable">The Pokémon could be opened with <c>pokemon.edit()</c> once added.</param>
/// <param name="Nickname">The species name when the Pokémon has no nickname.</param>
/// <param name="Types">Type ids, named by <c>game.types()</c>. One entry for a single-type Pokémon.</param>
public record PokemonPreview(
    int? SpeciesId,
    int SpeciesIndex,
    string Species,
    bool IsUnknown,
    bool Editable,
    PokemonForm Form,
    string Nickname,
    int Level,
    bool IsShiny,
    PokemonGender Gender,
    bool IsEgg,
    int[] Types);

/// <summary>
/// A Pokémon file as <c>box.addFromFile</c> would add it to the loaded save, returned by <c>box.previewFile</c>.
/// </summary>
/// <param name="Pokemon">The Pokémon as it would arrive in the loaded save.</param>
/// <param name="Unofficial">No game can move this Pokémon into the loaded save. <c>box.addFromFile</c> needs <c>allowUnofficial</c> to add it.</param>
/// <param name="Changes">The fields the conversion changes, in the order of <see cref="TransferField"/>.</param>
/// <param name="Legality">PKHeX's legality check of the Pokémon as it would arrive, judged in the loaded save. Null when the save has no legality check, such as a ROM hack's.</param>
public record FilePreview(PokemonPreview Pokemon, bool Unofficial, TransferChange[] Changes, Legality? Legality);

/// <summary>
/// How <c>box.addFromFile</c> adds a Pokémon file.
/// </summary>
/// <param name="AllowUnofficial">Add a Pokémon no game can move into the loaded save, converted as <c>box.previewFile</c> shows. Without it, such a file fails with <c>conversion-failed</c>.</param>
public record AddFromFileOptions(bool AllowUnofficial = false);
