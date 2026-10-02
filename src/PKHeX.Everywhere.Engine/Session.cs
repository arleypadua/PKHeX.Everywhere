using System.Buffers;
using System.Text;
using System.Text.Json;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine;

public sealed class Session
{
    public static Session Current { get; } = new();

    private readonly List<Invoker> _handlers = [];
    private readonly List<EventWriter> _eventWriters = [];
    private readonly AsyncLocal<CommandScope?> _command = new();

    public Game? Game { get; private set; }
    public string? FileName { get; private set; }
    internal IReadOnlyList<Encounter>? Encounters { get; set; }
    internal Draft? Draft { get; set; }

    public event Action<string[]>? Changed;
    public event Action? GameChanged;
    public event Action<IEngineEvent>? Published;

    public void Load(Game game, string? fileName)
    {
        if (Game is not null) Game.Trainer.PokemonsChanged -= HandlePokemonsChanged;

        Game = game;
        Encounters = null;
        Draft = null;
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
        Encounters = null;
        Draft = null;
        FileName = null;

        Invalidate(Topics.All);
        GameChanged?.Invoke();
    }

    internal IReadOnlyList<Invoker> Handlers => _handlers;

    /// <summary>
    /// Dispatches calls and writes events declared in another assembly. Pass that assembly's generated
    /// <c>HandlerRegistry.TryInvoke</c> and <c>HandlerRegistry.TryWriteEvent</c>.
    /// </summary>
    public void AddHandlers(Invoker handlers, EventWriter events)
    {
        _handlers.Add(handlers);
        _eventWriters.Add(events);
    }

    /// <summary>
    /// The JSON JS receives for an event, or null when no assembly attached to the Session declares it.
    /// </summary>
    public string? Serialize(IEngineEvent engineEvent)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            if (!HandlerRegistry.TryWriteEvent(engineEvent, writer) && !_eventWriters.Any(write => write(engineEvent, writer)))
                return null;
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    public void Invalidate(params string[] topics)
    {
        if (_command.Value is null) Changed?.Invoke(topics);
    }

    /// <summary>
    /// Reports Topics a running command writes beyond its declared ones, when they depend on the save.
    /// </summary>
    internal void AlsoWrote(params string[] topics) => _command.Value?.Written.AddRange(topics);

    internal void Raise(IEngineEvent engineEvent)
    {
        if (_command.Value is { } command) command.Raised.Add(engineEvent);
        else Published?.Invoke(engineEvent);
    }

    /// <summary>
    /// Publishes an event at once, even inside a command, for something that happened whether or not the command succeeds.
    /// </summary>
    internal void PublishNow(IEngineEvent engineEvent) => Published?.Invoke(engineEvent);

    // A command reports only the Topics it declares or passes to AlsoWrote, so the contract test catches declarations that are too narrow
    // instead of a Facade event covering for them.
    public T RunCommand<T>(string[] written, Func<T> command)
    {
        var scope = _command.Value = new CommandScope([.. written], []);
        T result;
        try
        {
            result = command();
        }
        finally
        {
            _command.Value = null;
        }

        Report(scope);
        return result;
    }

    // The scope is async-local, so calls that run while this command awaits report their own changes.
    public async Task<T> RunCommandAsync<T>(string[] written, Func<Task<T>> command)
    {
        var scope = _command.Value = new CommandScope([.. written], []);
        T result;
        try
        {
            result = await command();
        }
        finally
        {
            _command.Value = null;
        }

        Report(scope);
        return result;
    }

    private void Report(CommandScope scope)
    {
        Invalidate(scope.Written.Distinct().ToArray());
        foreach (var engineEvent in scope.Raised) Published?.Invoke(engineEvent);
    }

    public Game RequireGame() =>
        Game ?? throw new EngineException(ErrorCodes.NoSave, "No save is loaded.");

    private void HandlePokemonsChanged(PokemonSource source) => Invalidate(source switch
    {
        PokemonSource.Party => Topics.Party,
        PokemonSource.Box => Topics.Box,
        _ => Topics.All,
    });

    private sealed record CommandScope(List<string> Written, List<IEngineEvent> Raised);
}
