using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

/// <summary>
/// The values a Pokémon in this save can hold, for the lists that don't depend on the Pokémon.
/// </summary>
public class GameOptions
{
    private readonly IGameDataSource _data;

    public GameOptions(SaveFile saveFile) : this(new PKHeXGameData(saveFile))
    {
    }

    internal GameOptions(IGameDataSource data) => _data = data;

    public IReadOnlyList<Choice> Natures => _data.Natures;
    public IReadOnlyList<Choice> Balls => _data.Balls;
    public IReadOnlyList<Choice> Languages => _data.Languages;
    public IReadOnlyList<Choice> HeldItems => _data.HeldItems;
    public IReadOnlyList<Choice> OriginGames => _data.OriginGames;
    public IReadOnlyList<Choice> Moves => _data.Moves;

    public IReadOnlyList<Choice> Types => GameInfo.Strings.types
        .Take((int)MoveType.Fairy + 1)
        .Select((name, id) => new Choice(id, name))
        .ToArray();
}
