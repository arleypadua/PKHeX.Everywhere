using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// What a CFRU save can store: the species, items and moves its tables map, its ball table, and FireRed's languages and origin games.
/// Its Pokémon read nature, ability and gender from the PID or the species, and its stats use the official games' base stats.
/// </summary>
internal sealed class CfruGameData : IGameDataSource
{
    private static readonly PKHeXGameData FireRed = new(BlankSaveFile.Get(GameVersion.FR));

    private static readonly Choice[] NumberedLocations =
        Enumerable.Range(0, byte.MaxValue + 1).Select(location => new Choice(location, $"Location #{location}")).ToArray();

    private static readonly Choice[] CfruMoveChoices = CfruMoves.All
        .Select(move => new Choice(move, GameInfo.Strings.movelist[move]))
        .OrderBy(move => move.Name)
        .ToArray();

    private static readonly Choice[] CfruBalls = GameInfo.Sources.BallDataSource
        .Where(ball => ball.Value != (int)Ball.None && CfruPokemon.Balls.Contains((Ball)ball.Value))
        .Select(ball => new Choice(ball.Value, ball.Text))
        .ToArray();

    private readonly bool _fireRedMetLocations;

    public CfruGameData(CfruSave save, bool fireRedMetLocations)
    {
        _fireRedMetLocations = fireRedMetLocations;
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
        Locked = fireRedMetLocations
            ? FrozenSet.Create(PokemonField.Nature, PokemonField.Ability, PokemonField.Gender)
            : FrozenSet.Create(PokemonField.Nature, PokemonField.Ability, PokemonField.Gender, PokemonField.MetLocation);
    }

    public IReadOnlyList<Choice> Species { get; }
    public IReadOnlyList<Choice> Items { get; }
    public IReadOnlyList<Choice> HeldItems { get; }
    public IReadOnlyList<Choice> Moves => CfruMoveChoices;
    public IReadOnlyList<Choice> Balls => CfruBalls;
    public IReadOnlyList<Choice> Natures => FireRed.Natures;
    public IReadOnlyList<Choice> Languages => FireRed.Languages;
    public IReadOnlyList<Choice> OriginGames => FireRed.OriginGames;

    public IReadOnlyList<Choice> MetLocations(GameVersion origin, bool egg = false) =>
        _fireRedMetLocations ? FireRed.MetLocations(GameVersion.FR, egg) : NumberedLocations;

    public string? NameOf(GameDataKind kind, int id) => null;

    public IReadOnlySet<PokemonField> Locked { get; }

    public bool StatsApproximate => true;
}
