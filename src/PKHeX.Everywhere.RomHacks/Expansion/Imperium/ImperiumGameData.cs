using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.RomHacks.Expansion.Imperium;

// Imperium stores the nature apart from the PID, so unlike CFRU's it can change.
// Its map sections go past Emerald's, so met locations are named by number and locked, as Unbound's are.
internal sealed class ImperiumGameData : IGameDataSource
{
    private static readonly PKHeXGameData Emerald = new(BlankSaveFile.Get(GameVersion.E));

    private static readonly Choice[] NumberedLocations =
        Enumerable.Range(0, byte.MaxValue + 1).Select(location => new Choice(location, $"Location #{location}")).ToArray();

    private static readonly Choice[] MoveChoices = ImperiumMoveTable.Map.All
        .Select(move => new Choice(move, GameInfo.Strings.movelist[move]))
        .OrderBy(move => move.Name)
        .ToArray();

    private static readonly Choice[] BallChoices = GameInfo.Sources.BallDataSource
        .Where(ball => ball.Value != (int)Ball.None && ImperiumPokemon.Balls.Contains((Ball)ball.Value))
        .Select(ball => new Choice(ball.Value, ball.Text))
        .ToArray();

    private readonly ImperiumSave _save;

    public ImperiumGameData(ImperiumSave save)
    {
        _save = save;
        var itemNames = GameInfo.Strings.GetItemStrings(save.Context, save.Version);

        Species = save.SpeciesMap.Species
            .Select(species => new Choice(species, GameInfo.Strings.specieslist[species]))
            .OrderBy(species => species.Name)
            .ToArray();
        Items = save.ItemMap.Items
            .Prepend((ushort)0)
            .Select(item => new Choice(item, itemNames[item]))
            .ToArray();
        HeldItems = save.ItemMap.HeldItems
            .Select(item => new Choice(item, itemNames[item]))
            .OrderBy(item => item.Name)
            .Prepend(new Choice(0, itemNames[0]))
            .ToArray();
    }

    public IReadOnlyList<Choice> Species { get; }
    public IReadOnlyList<Choice> Items { get; }
    public IReadOnlyList<Choice> HeldItems { get; }
    public IReadOnlyList<Choice> Moves => MoveChoices;
    public IReadOnlyList<Choice> Balls => BallChoices;
    public IReadOnlyList<Choice> Natures => Emerald.Natures;
    public IReadOnlyList<Choice> Languages => Emerald.Languages;
    public IReadOnlyList<Choice> OriginGames => Emerald.OriginGames;

    public IReadOnlyList<Choice> MetLocations(GameVersion origin, bool egg = false) => NumberedLocations;

    public string? NameOf(GameDataKind kind, int id) => kind switch
    {
        GameDataKind.Species => _save.SpeciesMap.NameOf(id),
        GameDataKind.Move => ImperiumMoveTable.Map.NameOf(id),
        GameDataKind.Item or GameDataKind.HeldItem => _save.ItemMap.UnknownIndex(id) is { } index ? $"Unknown item #{index}" : null,
        _ => null,
    };

    public IReadOnlySet<PokemonField> Locked { get; } = new[] { PokemonField.Gender, PokemonField.MetLocation }.ToFrozenSet();

    public bool StatsApproximate => true;

    public bool StoresAbilitySlot => true;
}
