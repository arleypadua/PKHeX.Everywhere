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
