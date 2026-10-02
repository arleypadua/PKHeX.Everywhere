using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class CapabilitiesTests
{
    private static readonly byte[] Marked = Enumerable.Repeat((byte)0xC4, 0x2345).ToArray();

    static CapabilitiesTests() => SaveFormats.Register(new EmeraldWithoutCapabilities());

    private static Game Vanilla() => Game.LoadFrom(SaveFilePath.Emerald);

    private static Game WithoutCapabilities() => Game.LoadFrom(Marked);

    [Fact]
    public void AVanillaSaveHasEveryCapabilityAndNoFormat()
    {
        var game = Vanilla();

        game.Capabilities.Should().BeEquivalentTo(Enum.GetValues<Capability>());
        game.Format.Should().BeNull();
    }

    [Fact]
    public void ASaveFormatDescribesItselfAndTurnsCapabilitiesOff()
    {
        var game = WithoutCapabilities();

        game.Format.Should().Be(new SaveFormatDescription("no-capabilities", "No Capabilities", GameVersion.E));
        game.Capabilities.Should().BeEmpty();
    }

    [Fact]
    public void RequiringACapabilityThatIsOffThrows()
    {
        var game = WithoutCapabilities();

        game.Invoking(g => g.Require(Capability.Showdown)).Should().Throw<CapabilityNotSupportedException>()
            .Which.Capability.Should().Be(Capability.Showdown);
        Vanilla().Invoking(g => g.Require(Capability.Showdown)).Should().NotThrow();
    }

    [Fact]
    public void ShowdownIsOff()
    {
        var trainer = WithoutCapabilities().Trainer;

        trainer.Party.Pokemons[0].Invoking(p => p.Showdown()).Should().Throw<CapabilityNotSupportedException>();
        trainer.Party.Invoking(p => p.Showdown()).Should().Throw<CapabilityNotSupportedException>();
        trainer.PokemonBox.Invoking(b => b.Showdown()).Should().Throw<CapabilityNotSupportedException>();
    }

    [Fact]
    public void EncountersAreOff()
    {
        var repository = WithoutCapabilities().PokemonRepository;

        repository.Invoking(r => r.EncounterVersions()).Should().Throw<CapabilityNotSupportedException>();
        repository.Invoking(r => r.FindEncounter(GameVersion.E, Species.Zigzagoon)).Should().Throw<CapabilityNotSupportedException>();
    }

    [Fact]
    public async Task AutoLegalityIsOffAndLeavesThePokemonAsItWas()
    {
        var pokemon = WithoutCapabilities().Trainer.Party.Pokemons[0];
        var before = pokemon.Pkm.Data.ToArray();

        await pokemon.Awaiting(p => p.ApplyLegalAsync()).Should().ThrowAsync<CapabilityNotSupportedException>();

        pokemon.Pkm.Data.ToArray().Should().Equal(before);
    }

    [Fact]
    public void DetailsLeaveLegalityOutWhenItIsOff()
    {
        WithoutCapabilities().Trainer.Party.Pokemons[0].Details().Legality.Should().BeNull();
        Vanilla().Trainer.Party.Pokemons[0].Details().Legality.Should().NotBeNull();
    }

    [Fact]
    public void EventsAreAbsentWhenOff()
    {
        WithoutCapabilities().Events.Should().BeNull();
        Vanilla().Events.Should().NotBeNull();
    }

    private sealed class EmeraldWithoutCapabilities : ISaveFormat
    {
        public string Id => "no-capabilities";
        public string Name => "No Capabilities";
        public GameVersion BaseGame => GameVersion.E;
        public IReadOnlySet<Capability> Capabilities { get; } = new HashSet<Capability>();
        public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => data.SequenceEqual(Marked) ? SaveFormatMatch.Certain : SaveFormatMatch.No;
        public SaveFile Load(byte[] data) => SaveUtil.GetSaveFile(SaveFilePath.Emerald)!;
    }
}
