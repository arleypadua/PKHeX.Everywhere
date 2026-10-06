using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// The trainer's gender, as <c>trainer.get</c> returns it and <c>trainer.setGender</c> takes it.
/// </summary>
public enum TrainerGender
{
    Male,
    Female,
}

/// <summary>
/// The trainer's identity and currencies, returned by <c>trainer.get</c>.
/// </summary>
/// <param name="Id">The trainer ID and secret ID as <c>TID/SID</c>, in the format the game displays them.</param>
/// <param name="MaxNameLength">The longest name the game allows. <c>trainer.setName</c> cuts longer names to this length.</param>
/// <param name="Gender">The trainer's gender, or null for games without one: Red, Green, Blue, Yellow, Gold, Silver and Pokémon Stadium. <c>trainer.setGender</c> fails with <c>not-in-game</c> for those.</param>
/// <param name="Money">The trainer's money, or null when the save has none. <c>trainer.setMoney</c> accepts 0 to 999999 and caps it at the game's maximum.</param>
/// <param name="BattlePoints">The trainer's Battle Points, or null outside Generation 4 and 6 saves. <c>trainer.setBattlePoints</c> accepts 0 to 65535.</param>
/// <param name="Rival">The rival's name, or null when the engine can't read one for this game.</param>
public record TrainerCard(
    string Id,
    string Name,
    int MaxNameLength,
    TrainerGender? Gender,
    uint? Money,
    int? BattlePoints,
    string? Rival);

public static class TrainerMapping
{
    public static TrainerCard ToTrainerCard(this Game game) => new(
        game.Trainer.Id.ToString(),
        game.Trainer.Name,
        game.SaveFile.MaxStringLengthTrainer,
        game.Trainer.HasGender ? game.Trainer.Gender.ToTrainerGender() : null,
        game.Trainer.Money.IsSupported ? game.Trainer.Money.Amount : null,
        game.BattlePoints.IsSupported(out var supported) ? supported.BattlePoints : null,
        game.Trainer.RivalName);

    public static TrainerGender ToTrainerGender(this Gender gender) => gender.Equals(Gender.Female) ? TrainerGender.Female : TrainerGender.Male;

    public static Gender ToGender(this TrainerGender gender) => gender == TrainerGender.Female ? Gender.Female : Gender.Male;
}

/// <summary>
/// A gym badge, as <c>trainer.badges</c> lists it.
/// </summary>
/// <param name="Name">The badge's English name without "Badge", such as <c>Boulder</c>.</param>
/// <param name="Earned">Whether the trainer has the badge.</param>
public record Badge(string Name, bool Earned);

public static class BadgeMapping
{
    public static Badge[] ToDto(this Badges badges) => badges.All.Select(b => new Badge(b.Name, b.Earned)).ToArray();
}
