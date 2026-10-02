using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

/// <summary>
/// The values a Pokémon in this save can hold, for the lists that don't depend on the Pokémon.
/// </summary>
public class GameOptions(SaveFile saveFile)
{
    private readonly Lazy<FilteredGameDataSource> _source = new(() => new FilteredGameDataSource(saveFile, GameInfo.Sources));
    private Choice[]? _moves;

    public IReadOnlyList<Choice> Natures => saveFile.Generation >= 3 ? ToChoices(_source.Value.Natures) : [];
    public IReadOnlyList<Choice> Balls => ToChoices(_source.Value.Balls.Where(ball => ball.Value > 0));
    public IReadOnlyList<Choice> Languages => saveFile.Generation >= 3 ? ToChoices(_source.Value.Languages) : [];
    public IReadOnlyList<Choice> HeldItems => ToChoices(_source.Value.Items);
    public IReadOnlyList<Choice> OriginGames => saveFile.Generation >= 3 ? ToChoices(_source.Value.Games) : [];
    public IReadOnlyList<Choice> Moves => _moves ??= MoveIds()
        .Where(move => move != (int)Move.None)
        .Select(move => MoveRepository.Instance.GetMove((ushort)move))
        .OrderBy(move => move.Name)
        .Select(move => new Choice(move.Id, move.Name))
        .ToArray();

    private IEnumerable<int> MoveIds() => saveFile is IMoveList list
        ? list.Moves.Select(move => (int)move)
        : _source.Value.Moves.Select(move => move.Value);

    private static Choice[] ToChoices(IEnumerable<ComboItem> items) => items.Select(item => new Choice(item.Value, item.Text)).ToArray();
}
