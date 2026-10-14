using System.Collections.Immutable;
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

    public IImmutableList<GameVersionDefinition> EncounterVersions()
    {
        _game.Require(Capability.Encounters);
        return _game.AvailableVersions;
    }

    public IEnumerable<Encounter> FindEncounter(GameVersion version, Species species, bool? shiny = null, bool? egg = null)
    {
        _game.Require(Capability.Encounters);
        var settings = new SearchSettings
        {
            Context = _saveFile.Context,
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

    public IEnumerable<EventEncounter> FindEvents(Species? species = null)
    {
        _game.Require(Capability.Encounters);
        var all = species is { } one
            ? [one]
            : _game.SpeciesRepository.AllGameSpecies.Where(SpeciesDefinition.IsSome).Select(s => s.Species);
        var versions = EventVersions();

        return SaveRules.Under(_saveFile, () => all
            .SelectMany(s => versions.SelectMany(version => GiftsOf(s, version)))
            .Distinct()
            .Select(gift => (Gift: gift, Sample: EventEncounter.Generate(gift, _saveFile)))
            .Where(e => e.Sample is not null)
            .Select(e => new EventEncounter(e.Gift, e.Sample!, _game))
            .ToList());
    }

    // A Gen 2 save holds Gen 1 Pokémon through the Time Capsule, which the Korean games don't have.
    private GameVersion[] EventVersions() => _saveFile switch
    {
        { Generation: 1 } => [GameVersion.RD, GameVersion.GN, GameVersion.BU, GameVersion.YW],
        SAV2 { Korean: true } => [GameVersion.GD, GameVersion.SI],
        { Generation: 2 } => [GameVersion.RD, GameVersion.GN, GameVersion.BU, GameVersion.YW, GameVersion.GD, GameVersion.SI, GameVersion.C],
        { Generation: 3 } => [GameVersion.R, GameVersion.S, GameVersion.E, GameVersion.FR, GameVersion.LG],
        _ => GameUtil.GetVersionsInGeneration(_saveFile.Context, _saveFile.Version),
    };

    private List<IEncounterable> GiftsOf(Species species, GameVersion version)
    {
        var template = _saveFile.BlankPKM.Clone();
        template.Species = (ushort)species;
        template.Version = version;
        var origin = new EvolutionOrigin(template.Species, version.Context, version.Generation, 1, 100, OriginOptions.EncounterTemplate);
        var chain = EvolutionChain.GetOriginChain(template, origin);
        if (chain.Length == 0)
            return [];

        // DS cards are Mystery gifts too, but they go through the save's Mystery Gift album.
        return EncounterGenerator.GetGenerator(version, version.Generation)
            .GetPossible(template, chain, version, EncounterTypeGroup.Mystery)
            .Where(gift => gift.Species == template.Species && gift is not MysteryGift)
            .Where(gift => _saveFile.Personal.IsPresentInGame(gift.Species, gift.Form))
            .ToList();
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

    public Pokemon ConvertToPokemon() => new(Generate(), _game);

    internal virtual PKM Generate()
    {
        var save = _game.SaveFile;
        return EntityConverter.ConvertToType(Data.ConvertToPKM(save), save.PKMType, out var result)
            ?? throw new InvalidOperationException($"Can't convert {Species} to {save.PKMType.Name}: {result}");
    }

    public class Range(int min, int max)
    {
        public string Format => min != max
            ? $"{min}-{max}"
            : min.ToString();
    }
}