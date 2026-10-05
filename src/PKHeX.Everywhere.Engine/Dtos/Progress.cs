using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// How far the trainer has got, returned by <c>game.progress</c>.
/// </summary>
/// <param name="PlayTime">The play time the save records, or null when the save has none, such as a Stadium or Box save.</param>
/// <param name="Badges">The gym badges earned, or null when the engine can't read them for the game. Games from Sun and Moon on report null, except Brilliant Diamond and Shining Pearl.</param>
/// <param name="Pokedex">The Pokédex counts, or null when the save has no Pokédex or the engine can't read it.</param>
public record GameProgress(PlayTime? PlayTime, BadgeCount? Badges, PokedexCount? Pokedex);

/// <summary>
/// Hours, minutes and seconds played, as the game shows them.
/// </summary>
public record PlayTime(int Hours, int Minutes, int Seconds);

/// <summary>
/// Gym badges earned out of the badges the game has.
/// </summary>
/// <param name="Total">The badges the game has, such as 16 for Gold, Silver, Crystal, HeartGold and SoulSilver.</param>
public record BadgeCount(int Earned, int Total);

/// <summary>
/// The number of species the Pokédex has seen and caught, as PKHeX counts them.
/// </summary>
public record PokedexCount(int Seen, int Caught);

public static class ProgressMapping
{
    public static GameProgress ToDto(this Progress progress) => new(
        progress.PlayTime is { } time ? new PlayTime(time.Hours, time.Minutes, time.Seconds) : null,
        progress.Badges is { } badges ? new BadgeCount(badges.Earned, badges.Total) : null,
        progress.Pokedex is { } pokedex ? new PokedexCount(pokedex.Seen, pokedex.Caught) : null);
}
