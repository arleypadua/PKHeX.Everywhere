using System.Text.Json;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class TrainerHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void GetReturnsTheTrainerOfTheSave(string saveFile)
    {
        var session = Loaded(saveFile);

        var trainer = Value(Dispatch(session, "trainer.get", "[]"))!;

        var game = session.Game!;
        trainer.ToJsonString().Should().Be(JsonSerializer.Serialize(new
        {
            id = game.Trainer.Id.ToString(),
            name = game.Trainer.Name,
            maxNameLength = game.SaveFile.MaxStringLengthTrainer,
            gender = game.Trainer.Gender == PKHeX.Facade.Gender.Female ? "female" : "male",
            money = game.Trainer.Money.IsSupported ? game.Trainer.Money.Amount : (uint?)null,
            battlePoints = game.BattlePoints.IsSupported(out var supported) ? supported.BattlePoints : (int?)null,
            rival = game.Trainer.RivalName,
        }));
    }

    [Fact]
    public void GetReturnsNullMoneyWhenTheSaveDoesNotSupportIt() =>
        Value(Dispatch(LegendsZA(), "trainer.get", "[]"))!["money"].Should().BeNull();

    [Fact]
    public void GetReturnsNullBattlePointsWhenTheSaveHasNone() =>
        Value(Dispatch(Loaded(SaveFilePath.Emerald), "trainer.get", "[]"))!["battlePoints"].Should().BeNull();

    [Fact]
    public void GetReturnsNullRivalWhenTheSaveHasNone() =>
        Value(Dispatch(Loaded(SaveFilePath.Emerald), "trainer.get", "[]"))!["rival"].Should().BeNull();

    [Fact]
    public void GetReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "trainer.get", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void SetNameChangesTheName(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatch(session, "trainer.setName", Args("Ash"))).Should().BeNull();

        Trainer(session)["name"]!.GetValue<string>().Should().Be("Ash");
        session.Game!.SaveAndReload(reloaded => reloaded.Trainer.Name.Should().Be("Ash"));
    }

    [Theory]
    [SupportedSaveFiles]
    public void SetGenderChangesTheGender(string saveFile)
    {
        var session = Loaded(saveFile);
        var expected = Trainer(session)["gender"]!.GetValue<string>() == "male" ? "female" : "male";

        Value(Dispatch(session, "trainer.setGender", Args(expected))).Should().BeNull();

        Trainer(session)["gender"]!.GetValue<string>().Should().Be(expected);
    }

    [Theory]
    [SupportedSaveFiles]
    public void SetMoneyChangesTheMoney(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatch(session, "trainer.setMoney", Args(1234))).Should().BeNull();

        Trainer(session)["money"]!.GetValue<uint>().Should().Be(1234);
        session.Game!.SaveAndReload(reloaded => reloaded.Trainer.Money.Amount.Should().Be(1234));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1_000_000)]
    public void SetMoneyFailsWithOutOfRangeOutsideTheLimits(int amount) =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "trainer.setMoney", Args(amount))).Should().Be("out-of-range");

    [Fact]
    public void SetMoneyFailsWithNotInGameWhenTheSaveDoesNotSupportIt() =>
        Error(Dispatch(LegendsZA(), "trainer.setMoney", Args(1234))).Should().Be("not-in-game");

    [Fact]
    public void SetBattlePointsChangesTheBattlePoints()
    {
        var session = Loaded(SaveFilePath.HgSs);

        Value(Dispatch(session, "trainer.setBattlePoints", Args(321))).Should().BeNull();

        Trainer(session)["battlePoints"]!.GetValue<int>().Should().Be(321);
        session.Game!.SaveAndReload(reloaded =>
        {
            reloaded.BattlePoints.IsSupported(out var supported).Should().BeTrue();
            supported.BattlePoints.Should().Be(321);
        });
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(65536)]
    public void SetBattlePointsFailsWithOutOfRangeOutsideTheLimits(int value) =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "trainer.setBattlePoints", Args(value))).Should().Be("out-of-range");

    [Fact]
    public void SetBattlePointsFailsWithNotInGameWhenTheSaveHasNone() =>
        Error(Dispatch(Loaded(SaveFilePath.Emerald), "trainer.setBattlePoints", Args(1))).Should().Be("not-in-game");

    [Theory]
    [InlineData("setName", "Ash")]
    [InlineData("setGender", "female")]
    [InlineData("setMoney", 1234)]
    [InlineData("setBattlePoints", 1)]
    public void SettersChangeTheTrainerTopic(string command, object value)
    {
        var session = Loaded(SaveFilePath.HgSs);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Dispatch(session, $"trainer.{command}", Args(value));

        changes.Should().BeEquivalentTo([new[] { Topics.Trainer }]);
    }

    private static System.Text.Json.Nodes.JsonNode Trainer(Session session) =>
        Value(Dispatch(session, "trainer.get", "[]"))!;

    private static Session LegendsZA()
    {
        var session = new Session();
        session.Load(Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.ZA)), "za.bin");
        return session;
    }
}
