using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

public enum TrainerGender
{
    Male,
    Female,
}

public record TrainerCard(
    string Id,
    string Name,
    int MaxNameLength,
    TrainerGender Gender,
    uint? Money,
    int? BattlePoints,
    string? Rival);

public static class TrainerMapping
{
    public static TrainerCard ToTrainerCard(this Game game) => new(
        game.Trainer.Id.ToString(),
        game.Trainer.Name,
        game.SaveFile.MaxStringLengthTrainer,
        game.Trainer.Gender.ToTrainerGender(),
        game.Trainer.Money.IsSupported ? game.Trainer.Money.Amount : null,
        game.BattlePoints.IsSupported(out var supported) ? supported.BattlePoints : null,
        game.Trainer.RivalName);

    public static TrainerGender ToTrainerGender(this Gender gender) => gender.Equals(Gender.Female) ? TrainerGender.Female : TrainerGender.Male;

    public static Gender ToGender(this TrainerGender gender) => gender == TrainerGender.Female ? Gender.Female : Gender.Male;
}
