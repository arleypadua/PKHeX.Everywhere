using PKHeX.Facade;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine;

public sealed class Session
{
    public static Session Current { get; } = new();

    private bool _inCommand;

    public Game? Game { get; private set; }
    public string? FileName { get; private set; }

    public event Action<string[]>? Changed;
    public event Action? GameChanged;

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

    public void Invalidate(params string[] topics)
    {
        if (!_inCommand) Changed?.Invoke(topics);
    }

    // A command reports only the Topics it declares, so the contract test catches declarations that are too narrow
    // instead of a Facade event covering for them.
    internal T RunCommand<T>(string[] written, Func<T> command)
    {
        T result;
        _inCommand = true;
        try
        {
            result = command();
        }
        finally
        {
            _inCommand = false;
        }

        Invalidate(written);
        return result;
    }

    internal Game RequireGame() =>
        Game ?? throw new EngineException(ErrorCodes.NoSave, "No save is loaded.");

    private void HandlePokemonsChanged(PokemonSource source) => Invalidate(source switch
    {
        PokemonSource.Party => Topics.Party,
        PokemonSource.Box => Topics.Box,
        _ => Topics.All,
    });
}
