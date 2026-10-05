using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class TrainerHandlers
{
    private const int MaxMoney = 999_999;

    [Query("trainer.get", Topics.Trainer)]
    public static TrainerCard Get(Game game) => game.ToTrainerCard();

    [Command("trainer.setName", Topics.Trainer)]
    public static void SetName(Game game, string name) => game.Trainer.Name = name;

    [Command("trainer.setGender", Topics.Trainer)]
    public static void SetGender(Game game, TrainerGender gender) => game.Trainer.Gender = gender.ToGender();

    [Command("trainer.setMoney", Topics.Trainer)]
    public static void SetMoney(Game game, int amount)
    {
        if (!game.Trainer.Money.IsSupported)
            throw new EngineException(ErrorCodes.NotInGame, "This save has no money.");
        RequireInRange(amount, MaxMoney);

        game.Trainer.Money.Set((uint)amount);
    }

    [Command("trainer.setBattlePoints", Topics.Trainer)]
    public static void SetBattlePoints(Game game, int value)
    {
        if (!game.BattlePoints.IsSupported(out var supported))
            throw new EngineException(ErrorCodes.NotInGame, "This save has no Battle Points.");
        RequireInRange(value, ushort.MaxValue);

        supported.BattlePoints = value;
    }

    /// <summary>
    /// Lists the gym badges in game order. Gold, Silver and Crystal list the Johto badges, then the Kanto ones.
    /// Returns null outside Generations 1 to 3 and for ROM hacks.
    /// </summary>
    [Query("trainer.badges", Topics.Trainer, Topics.Events)]
    public static Dtos.Badge[]? Badges(Game game) => game.Badges?.ToDto();

    /// <summary>
    /// Sets which gym badges the trainer has, one value per badge in the order <c>trainer.badges</c> lists them.
    /// Throws <c>not-supported</c> when <c>trainer.badges</c> returns null, and <c>out-of-range</c> when the number of values doesn't match.
    /// </summary>
    [Command("trainer.setBadges", Topics.Trainer, Topics.Events)]
    public static void SetBadges(Game game, bool[] earned)
    {
        var badges = game.Badges ?? throw new EngineException(ErrorCodes.NotSupported, "The engine can't write badges for this save.");
        try
        {
            badges.Set(earned);
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new EngineException(ErrorCodes.OutOfRange, e.Message, e);
        }
    }

    private static void RequireInRange(int value, int max)
    {
        if (value < 0 || value > max)
            throw new EngineException(ErrorCodes.OutOfRange, $"Value must be between 0 and {max}, got {value}.");
    }
}
