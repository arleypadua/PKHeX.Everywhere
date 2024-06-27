using System.Collections.Immutable;
using PKHeX.Core;
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

    public MoveDefinition GetMove(ushort id) => _moves[id];

    public List<MoveDefinition> PossibleMovesFor(Pokemon pokemon)
    {
        // consider the current set of moves, whenever they have been learnt by other sources
        var possibleCurrentMoves = pokemon.Legality().Info.Moves
            .Zip(pokemon.Moves.Values)
            .Where(m => m.Second.Move != MoveDefinition.None && m.First.Valid)
            .Select(m => m.Second.Move.Id);
        
        var learnSource = GameData.GetLearnSource(pokemon.Game.SaveFile.Version);
        var learnSet = learnSource.GetLearnset(pokemon.Pkm.Species, pokemon.Pkm.Form);
        
        var moves = learnSet.GetMoveRange(pokemon.Level)
            .ToImmutableArray().AddRange(possibleCurrentMoves)
            .ToHashSet();

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