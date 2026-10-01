using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

/// <summary>
/// The values a Pokémon in this save can hold, for the lists that don't depend on the Pokémon.
/// </summary>
public class GameOptions(SaveFile saveFile)
{
    private readonly Lazy<FilteredGameDataSource> _source = new(() => new FilteredGameDataSource(saveFile, GameInfo.Sources));

    public IReadOnlyList<Choice> Natures => saveFile.Generation >= 3 ? ToChoices(_source.Value.Natures) : [];
    public IReadOnlyList<Choice> Balls => ToChoices(_source.Value.Balls.Where(ball => ball.Value > 0));
    public IReadOnlyList<Choice> Languages => saveFile.Generation >= 3 ? ToChoices(_source.Value.Languages) : [];
    public IReadOnlyList<Choice> HeldItems => ToChoices(_source.Value.Items);

    private static Choice[] ToChoices(IEnumerable<ComboItem> items) => items.Select(item => new Choice(item.Value, item.Text)).ToArray();
}
