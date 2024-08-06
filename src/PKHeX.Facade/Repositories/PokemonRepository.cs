using PKHeX.Core;
using PKHeX.Core.Searching;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Repositories;

public class PokemonRepository
{
    private readonly SaveFile _saveFile;
    private readonly Game _game;

    internal PokemonRepository(Game game)
    {
        _game = game;
        _saveFile = game.SaveFile;
    }

    public IEnumerable<Encounter> FindEncounter(GameVersion version, Species species, bool? shiny = null, bool? egg = null)
    {
        var settings = new SearchSettings
        {
            Format = _saveFile.Generation,
            Generation = _saveFile.Generation,

            Species = (ushort)species,
            SearchShiny = shiny,
            SearchEgg = egg,

            Version = version, 
        };

        var personalInfo = _saveFile.Personal.GetFormEntry(settings.Species, 0);
        var formCount = personalInfo.FormCount;

        return Enumerable.Range(0, formCount)
            .Where(form => !FormInfo.IsBattleOnlyForm(settings.Species, (byte)form, _saveFile.BlankPKM.Format))
            .SelectMany(form => GetEncounterFor(settings, form));
    }

    private IEnumerable<Encounter> GetEncounterFor(SearchSettings settings, int form)
    {
        var template = _saveFile.BlankPKM.Clone();
        template.Species = settings.Species;
        template.Form = (byte)form;
        template.SetGender(template.GetSaneGender());
        
        EncounterMovesetGenerator.OptimizeCriteria(template, _saveFile);
        return EncounterMovesetGenerator
            .GenerateEncounters(template, settings.Moves.ToArray(), settings.GetVersions(_saveFile))
            .Where(e => settings.SearchShiny is null || e.IsShiny == settings.SearchShiny)
            .Where(e => settings.SearchEgg is null || e.IsEgg == settings.SearchEgg)
            .Where(p => _saveFile.Personal.IsPresentInGame(p.Species, p.Form))
            .Distinct()
            .Select(e => new Encounter(e, _game));
    }
}

public class Encounter
{
    private readonly Game _game;

    public Encounter() : this(default!, default!) { }

    internal Encounter(IEncounterable data, Game game)
    {
        Data = data;
        _game = game;
    }

    public IEncounterable Data { get; }

    public LocationDefinition Location => Data.IsEgg
        ? _game.LocationRepository.GetBy(Data.EggLocation, true)
        : _game.LocationRepository.GetBy(Data.Location);

    public ItemDefinition Ball => ItemRepository.GetBall(Data.FixedBall)
        ?? throw new InvalidOperationException($"Ball item not found: {Data.FixedBall}");

    public Range LevelRange => new(Data.LevelMin, Data.LevelMax);

    public Species Species => (Species)Data.Species;

    public FormDefinition? Form => FormRepository.GetFor(Species, Data.Context)
        .FirstOrDefault(f => f.Id == Data.Form);

    public GameVersionDefinition Version => GameVersionRepository.Instance.Get(Data.Version);

    public Pokemon ConvertToPokemon() => new(Data.ConvertToPKM(_game.SaveFile), _game);

    public class Range(int min, int max)
    {
        public string Format => min != max
            ? $"{min}-{max}"
            : min.ToString();
    }
}