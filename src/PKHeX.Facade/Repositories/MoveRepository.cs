using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Repositories;

public class MoveRepository
{
    public static readonly MoveRepository Instance = new();

    private readonly Dictionary<ushort, MoveDefinition> _moves;

    private MoveRepository()
    {
        _moves = GameInfo.Strings.movelist
            .Select((moveName, id) => (id: Convert.ToUInt16(id), moveName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new MoveDefinition(Convert.ToUInt16(x.id), x.moveName));
    }

    public MoveDefinition GetMove(ushort id) => GetMove(id, null);

    internal MoveDefinition GetMove(ushort id, IGameDataSource? data) =>
        _moves.TryGetValue(id, out var move) ? move
        : data?.NameOf(GameDataKind.Move, id) is { } name ? new MoveDefinition(id, name) { IsUnknown = true }
        : new MoveDefinition(id, $"Unknown ({id})");

    public List<MoveDefinition> AllMovesFor(Game game) =>
        game.Options.Moves.Select(m => GetMove((ushort)m.Id)).ToList();

    public List<MoveDefinition> PossibleMovesFor(Pokemon pokemon)
    {
        var saveFile = pokemon.Game.SaveFile;
        var version = saveFile.Version == GameVersion.BATREV
            ? saveFile.Context.GetSingleGameVersion()
            : saveFile.Version;
        var learnSource = GameData.GetLearnSource(version);
        var learnable = SpeciesRepository.Find(pokemon.Pkm.Species) is null
            ? []
            : learnSource.GetLearnset(pokemon.Pkm.Species, pokemon.Pkm.Form).GetMoveRange((byte)pokemon.Level).ToArray();
        if (learnable.Length == 0)
            return pokemon.Game.GameData.Moves
                .Select(move => GetMove((ushort)move.Id, pokemon.Game.GameData))
                .OrderBy(move => move.Name)
                .ToList();

        // consider the current set of moves, whenever they have been learnt by other sources
        var possibleCurrentMoves = pokemon.Legality().Info.Moves
            .Zip(pokemon.Moves.Values)
            .Where(m => m.Second.Move != MoveDefinition.None && m.First.Valid)
            .Select(m => m.Second.Move.Id);

        var moves = learnable.ToImmutableArray().AddRange(possibleCurrentMoves).ToHashSet();

        return _moves
            .Where(x => moves.Contains(x.Key))
            .Select(x => x.Value)
            .OrderBy(x => x.Name)
            .ToList();
    }
}

public record MoveDefinition(ushort Id, string Name)
{
    public static readonly MoveDefinition None = new((ushort)Move.None, $"({Move.None})");

    public bool IsUnknown { get; init; }

    public virtual bool Equals(MoveDefinition? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
};