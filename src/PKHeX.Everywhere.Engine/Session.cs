using PKHeX.Facade;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine;

public sealed class Session
{
    public static Session Current { get; } = new();

    private readonly List<Invoker> _handlers = [];
    private List<IEngineEvent>? _raised;
    private List<string>? _written;

    public Game? Game { get; private set; }
    public string? FileName { get; private set; }

    public event Action<string[]>? Changed;
    public event Action? GameChanged;
    public event Action<IEngineEvent>? Published;

    public void Load(Game game, string? fileName)
    {
        if (Game is not null) Game.Trainer.PokemonsChanged -= HandlePokemonsChanged;

        Game = game;
        FileName = string.IsNullOrWhiteSpace(fileName) ? FileName : fileName;
        game.Trainer.PokemonsChanged += HandlePokemonsChanged;

        Invalidate(Topics.All);
        GameChanged?.Invoke();
    }

    public void Close()
    {
        if (Game is null) return;

        Game.Trainer.PokemonsChanged -= HandlePokemonsChanged;
        Game = null;
        FileName = null;

        Invalidate(Topics.All);
        GameChanged?.Invoke();
    }

    internal IReadOnlyList<Invoker> Handlers => _handlers;

    /// <summary>
    /// Dispatches calls declared in another assembly. Pass that assembly's generated <c>HandlerRegistry.TryInvoke</c>.
    /// </summary>
    public void AddHandlers(Invoker handlers) => _handlers.Add(handlers);

    public void Invalidate(params string[] topics)
    {
        if (_raised is null) Changed?.Invoke(topics);
    }

    /// <summary>
    /// Reports Topics a running command writes beyond its declared ones, when they depend on the save.
    /// </summary>
    internal void AlsoWrote(params string[] topics) => _written?.AddRange(topics);

    internal void Raise(IEngineEvent engineEvent)
    {
        if (_raised is null) Published?.Invoke(engineEvent);
        else _raised.Add(engineEvent);
    }

    // A command reports only the Topics it declares or passes to AlsoWrote, so the contract test catches declarations that are too narrow
    // instead of a Facade event covering for them.
    public T RunCommand<T>(string[] written, Func<T> command)
    {
        T result;
        var raised = _raised = [];
        var reported = _written = [.. written];
        try
        {
            result = command();
        }
        finally
        {
            _raised = null;
            _written = null;
        }

        Invalidate(reported.Distinct().ToArray());
        foreach (var engineEvent in raised) Published?.Invoke(engineEvent);
        return result;
    }

    public Game RequireGame() =>
        Game ?? throw new EngineException(ErrorCodes.NoSave, "No save is loaded.");

    private void HandlePokemonsChanged(PokemonSource source) => Invalidate(source switch
    {
        PokemonSource.Party => Topics.Party,
        PokemonSource.Box => Topics.Box,
        _ => Topics.All,
    });
}
