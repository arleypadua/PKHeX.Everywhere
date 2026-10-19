using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.RomHacks.Expansion;

// An expansion hack stores the nature apart from the PID, so unlike CFRU's it can change.
// Its map sections go past Emerald's, so met locations are named by number and locked, as Unbound's are.
internal sealed class ExpansionGameData : IGameDataSource
{
    private static readonly PKHeXGameData Emerald = new(BlankSaveFile.Get(GameVersion.E));

    private static readonly Choice[] NumberedLocations =
        Enumerable.Range(0, byte.MaxValue + 1).Select(location => new Choice(location, $"Location #{location}")).ToArray();

    private readonly CfruSpeciesMap _species;
    private readonly CfruItemMap _items;
    private readonly MoveMap _moves;

    public ExpansionGameData(SaveFile save, CfruSpeciesMap species, CfruItemMap items, MoveMap moves, Ball[] balls)
    {
        _species = species;
        _items = items;
        _moves = moves;
        var itemNames = GameInfo.Strings.GetItemStrings(save.Context, save.Version);

        Species = species.Species
            .Select(id => new Choice(id, GameInfo.Strings.specieslist[id]))
            .OrderBy(choice => choice.Name)
            .ToArray();
        Items = items.Items
            .Prepend((ushort)0)
            .Select(item => new Choice(item, itemNames[item]))
            .ToArray();
        HeldItems = items.HeldItems
            .Select(item => new Choice(item, itemNames[item]))
            .OrderBy(item => item.Name)
            .Prepend(new Choice(0, itemNames[0]))
            .ToArray();
        Moves = moves.All
            .Select(move => new Choice(move, GameInfo.Strings.movelist[move]))
            .OrderBy(move => move.Name)
            .ToArray();
        Balls = GameInfo.Sources.BallDataSource
            .Where(ball => ball.Value != (int)Ball.None && balls.Contains((Ball)ball.Value))
            .Select(ball => new Choice(ball.Value, ball.Text))
            .ToArray();
    }

    public IReadOnlyList<Choice> Species { get; }
    public IReadOnlyList<Choice> Items { get; }
    public IReadOnlyList<Choice> HeldItems { get; }
    public IReadOnlyList<Choice> Moves { get; }
    public IReadOnlyList<Choice> Balls { get; }
    public IReadOnlyList<Choice> Natures => Emerald.Natures;
    public IReadOnlyList<Choice> Languages => Emerald.Languages;
    public IReadOnlyList<Choice> OriginGames => Emerald.OriginGames;

    public IReadOnlyList<Choice> MetLocations(GameVersion origin, bool egg = false) => NumberedLocations;

    public string? NameOf(GameDataKind kind, int id) => kind switch
    {
        GameDataKind.Species => _species.NameOf(id),
        GameDataKind.Move => _moves.NameOf(id),
        GameDataKind.Item or GameDataKind.HeldItem => _items.UnknownIndex(id) is { } index ? $"Unknown item #{index}" : null,
        _ => null,
    };

    public int ItemIndex(int item) => _items.ToIndex((ushort)item) ?? 0;

    public IReadOnlySet<PokemonField> Locked { get; } = new[] { PokemonField.Gender, PokemonField.MetLocation }.ToFrozenSet();

    public bool StatsApproximate => true;

    public bool StoresAbilitySlot => true;
}
