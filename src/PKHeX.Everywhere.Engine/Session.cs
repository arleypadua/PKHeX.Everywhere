using PKHeX.Facade;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine;

public sealed class Session
{
    public static Session Current { get; } = new();

    public Game? Game { get; private set; }
    public string? FileName { get; private set; }

    public event Action<string[]>? Changed;

    public void Load(Game game, string? fileName)
    {
        if (Game is not null) Game.Trainer.PokemonsChanged -= HandlePokemonsChanged;

        Game = game;
        FileName = string.IsNullOrWhiteSpace(fileName) ? FileName : fileName;
        game.Trainer.PokemonsChanged += HandlePokemonsChanged;

        Invalidate(Topics.All);
    }

    public void Invalidate(params string[] topics) => Changed?.Invoke(topics);

    internal Game RequireGame() =>
        Game ?? throw new EngineException(ErrorCodes.NoSave, "No save is loaded.");

    private void HandlePokemonsChanged(PokemonSource source) => Invalidate(source switch
    {
        PokemonSource.Party => Topics.Party,
        PokemonSource.Box => Topics.Box,
        _ => Topics.All,
    });
}
