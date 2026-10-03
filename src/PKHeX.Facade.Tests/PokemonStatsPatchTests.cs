using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonStatsPatchTests
{
    [Theory]
    [SupportedSaveFiles]
    public void ReportsTheStatInputsAndTheComputedStats(string saveFile)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];
        var pkm = pokemon.Pkm;
        var stats = pkm.GetStats(pkm.PersonalInfo);

        pokemon.Details().Should().BeEquivalentTo(new
        {
            Ivs = new StatValues(pkm.IV_HP, pkm.IV_ATK, pkm.IV_DEF, pkm.IV_SPA, pkm.IV_SPD, pkm.IV_SPE),
            Evs = new StatValues(pkm.EV_HP, pkm.EV_ATK, pkm.EV_DEF, pkm.EV_SPA, pkm.EV_SPD, pkm.EV_SPE),
            Stats = new StatValues(stats[0], stats[1], stats[2], stats[4], stats[5], stats[3]),
            pokemon.HiddenPower,
        });
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.LetsGoPikachu, SaveFilePath.LetsGoEevee])]
    public void HasNoAwakeningValuesOrCombatPowerOutsideLetsGo(string saveFile)
    {
        var details = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0].Details();

        details.Avs.Should().BeNull();
        details.CombatPower.Should().BeNull();
        details.CalculatedCombatPower.Should().BeNull();
    }

    [Theory]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    public void AppliesTheIvsAndEvs(string saveFile)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(
            Ivs: new StatPatch(1, 2, 3, 4, 5, 6),
            Evs: new StatPatch(Attack: 100, Speed: 252)));

        var details = pokemon.Details();
        details.Ivs.Should().Be(new StatValues(1, 2, 3, 4, 5, 6));
        details.Evs.Should().BeEquivalentTo(new { Attack = 100, Speed = 252 });
    }

    [Fact]
    public void ComputedStatsFollowTheInputs()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        pokemon.Update(new PokemonPatch(Ivs: new StatPatch(Attack: 0), Evs: new StatPatch(Attack: 0)));
        var before = pokemon.Details().Stats.Attack;

        pokemon.Update(new PokemonPatch(Ivs: new StatPatch(Attack: 31), Evs: new StatPatch(Attack: 252)));

        pokemon.Details().Stats.Attack.Should().BeGreaterThan(before);
    }

    [Fact]
    public void AppliesTheAwakeningValues()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(Avs: new StatPatch(Health: 200, Speed: 7)));

        pokemon.Details().Avs.Should().BeEquivalentTo(new { Health = 200, Speed = 7 });
    }

    [Fact]
    public void ChangingAStatInputRecalculatesTheCombatPower()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons[0];
        pokemon.Update(new PokemonPatch(CombatPower: 1));

        pokemon.Update(new PokemonPatch(Avs: new StatPatch(Attack: 200)));

        var details = pokemon.Details();
        details.CombatPower.Should().Be(((PB7)pokemon.Pkm).CalcCP);
        details.CalculatedCombatPower.Should().Be(details.CombatPower);
    }

    [Fact]
    public void AppliesTheCombatPowerAfterTheStatInputs()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(Avs: new StatPatch(Attack: 200), CombatPower: 1234));

        var details = pokemon.Details();
        details.CombatPower.Should().Be(1234);
        details.CalculatedCombatPower.Should().Be(((PB7)pokemon.Pkm).CalcCP);
    }

    [Fact]
    public void GameBoyGamesShareTheSpecialIv()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Crystal).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(Ivs: new StatPatch(SpecialAttack: 9)));

        pokemon.Details().Ivs.SpecialDefense.Should().Be(9);
    }

    [Theory]
    [InlineData(SaveFilePath.HgSs, "Ivs.Attack", "Attack IV")]
    [InlineData(SaveFilePath.Crystal, "Ivs.Health", "HP IV")]
    [InlineData(SaveFilePath.HgSs, "Evs.Speed", "Speed EV")]
    [InlineData(SaveFilePath.HgSs, "Avs", "Awakening values")]
    [InlineData(SaveFilePath.LetsGoPikachu, "Avs.Defense", "Defense AV")]
    [InlineData(SaveFilePath.HgSs, "CombatPower", "Combat Power")]
    public void RejectsAValueTheSaveCantHold(string saveFile, string field, string named)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];
        var before = pokemon.Details();
        var patch = field switch
        {
            "Ivs.Attack" => new PokemonPatch(Ivs: new StatPatch(Attack: 32)),
            "Ivs.Health" => new PokemonPatch(Ivs: new StatPatch(Health: (pokemon.Pkm.IV_HP + 1) % 16)),
            "Evs.Speed" => new PokemonPatch(Evs: new StatPatch(Speed: 256)),
            "Avs" => new PokemonPatch(Avs: new StatPatch(Health: 1)),
            "Avs.Defense" => new PokemonPatch(Avs: new StatPatch(Defense: 201)),
            _ => new PokemonPatch(CombatPower: 100),
        };

        var update = () => pokemon.Update(patch with { Nickname = "Sparky" });

        update.Should().Throw<InvalidPatchException>()
            .Where(e => e.Field == field && e.Message.StartsWith(named));
        pokemon.Details().Should().BeEquivalentTo(before);
    }
}
