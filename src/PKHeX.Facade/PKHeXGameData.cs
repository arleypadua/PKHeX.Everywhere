using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

/// <summary>
/// The Game data source of a save PKHeX reads on its own, built from PKHeX's data.
/// </summary>
public class PKHeXGameData : IGameDataSource
{
    private readonly SaveFile _save;
    private readonly Lazy<FilteredGameDataSource> _source;
    private readonly Lazy<Choice[]> _species;
    private readonly Lazy<Choice[]> _items;
    private readonly Lazy<Choice[]> _moves;

    public PKHeXGameData(SaveFile save)
    {
        _save = save;
        _source = new(() => new FilteredGameDataSource(save, GameInfo.Sources));
        _species = new(() => _source.Value.Species
            .Where(species => save.Personal.IsSpeciesInGame((ushort)species.Value))
            .Select(ToChoice)
            .ToArray());
        _items = new(GameItems);
        _moves = new(() => _source.Value.Moves
            .Select(move => move.Value)
            .Where(move => move != (int)Move.None)
            .Select(move => MoveRepository.Instance.GetMove((ushort)move))
            .OrderBy(move => move.Name)
            .Select(move => new Choice(move.Id, move.Name))
            .ToArray());
        Locked = save.Generation is 3 or 4 ? FrozenSet.Create(PokemonField.Nature) : FrozenSet<PokemonField>.Empty;
    }

    public virtual IReadOnlyList<Choice> Species => _species.Value;
    public virtual IReadOnlyList<Choice> Items => _items.Value;
    public virtual IReadOnlyList<Choice> HeldItems => ToChoices(_source.Value.Items);
    public virtual IReadOnlyList<Choice> Moves => _moves.Value;
    public virtual IReadOnlyList<Choice> Balls => ToChoices(_source.Value.Balls.Where(ball => ball.Value > 0));
    public virtual IReadOnlyList<Choice> Natures => _save.Generation >= 3 ? ToChoices(_source.Value.Natures) : [];
    public virtual IReadOnlyList<Choice> Languages => _save.Generation >= 3 ? ToChoices(_source.Value.Languages) : [];
    public virtual IReadOnlyList<Choice> OriginGames => _save.Generation >= 3 ? ToChoices(_source.Value.Games) : [];

    public virtual IReadOnlyList<Choice> MetLocations(GameVersion origin, bool egg = false) => _save.Generation <= 1
        ? []
        : GameInfo.GetLocationList(MetLocationVersion(origin, _save), _save.Context, egg)
            .DistinctBy(location => location.Value)
            .Select(ToChoice)
            .ToArray();

    public virtual string? NameOf(GameDataKind kind, int id) => null;

    public virtual IReadOnlySet<PokemonField> Locked { get; }

    public virtual bool StatsApproximate => false;

    public virtual bool StoresAbilitySlot => _save.Generation == 3;

    // Mirrors PKHeX's editor: an origin game without its own location list borrows the save's, then the format's.
    internal static GameVersion MetLocationVersion(GameVersion origin, SaveFile save)
    {
        if (GameUtil.GetMetLocationVersionGroup(origin) is not GameVersion.Invalid) return origin;

        return GameUtil.GetMetLocationVersionGroup(save.Version) is GameVersion.Invalid || save.Version is GameVersion.Any
            ? save.Context.GetSingleGameVersion()
            : save.Version;
    }

    private Choice[] GameItems()
    {
        var items = GameInfo.Strings.GetItemStrings(_save.Context, _save.Version)
            .Select((name, id) => new Choice(id, name))
            .Where(item => !string.IsNullOrEmpty(item.Name));

        // for whatever reason, Pokemon Crystal has this last item
        return _save.Version == GameVersion.C
            ? items.Where(item => item.Id != 255).Append(new Choice(255, "Collapsible bike")).ToArray()
            : items.ToArray();
    }

    private static Choice ToChoice(ComboItem item) => new(item.Value, item.Text);

    private static Choice[] ToChoices(IEnumerable<ComboItem> items) => items.Select(ToChoice).ToArray();
}
