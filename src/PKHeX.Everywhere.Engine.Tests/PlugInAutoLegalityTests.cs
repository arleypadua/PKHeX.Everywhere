using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInAutoLegalityTests : IDisposable
{
    private const string AutoLegality = "PKHeX.Web.Plugins.AutoLegality";
    private const string Legalize = $"{AutoLegality}.MakeLegalOnClick";
    private const string LegalizeOnChange = $"{AutoLegality}.MakeLegalOnChange";
    private const string LegalizeOnSave = $"{AutoLegality}.MakeLegalOnSave";
    private const string Timeout = "Timeout (seconds)";
    private const string ForceLevel100 = "Force lvl 100 from 50";

    public void Dispose() => Facade.AutoLegality.ApplyDefaultConfiguration();

    private static (Session Session, PlugInHost Host, List<PlugInRan> Ran) Hosted()
    {
        var session = Loaded(SaveFilePath.LetsGoPikachu);
        var host = new PlugInHost(session);
        var ran = new List<PlugInRan>();
        host.Ran += ran.Add;
        host.Register(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{AutoLegality}.dll")));
        host.UpdateSetting(AutoLegality, Timeout, new Settings.SettingValue.IntegerValue(15));
        return (session, host, ran);
    }

    private static (PokemonHandle At, Pokemon Pokemon) IllegalPikachu(Session session)
    {
        var party = session.Game!.Trainer.Party;
        var slot = party.Pokemons.ToList().FindIndex(p => p.Species == Species.Pikachu);
        var pikachu = party.Pokemons[slot];
        pikachu.ChangeLevel(50);
        party.Commit();
        pikachu.Legality().Valid.Should().BeFalse();
        return (PokemonHandle.Party(slot), pikachu);
    }

    private static string Call(string id, PokemonHandle at) => JsonSerializer.Serialize(new object[]
    {
        id, new { source = at.Source.ToString().ToLowerInvariant(), slot = at.Slot, box = at.Box },
    });

    [Fact]
    public void ListsLegalizeForThePokemonPlacement()
    {
        var (session, _, _) = Hosted();

        var actions = Value(Dispatch(session, "plugins.actions", Call("pokemon", PokemonHandle.Party(0))))!.AsArray();

        actions.Should().ContainSingle().Which.ToJsonString().Should().Be(new JsonObject
        {
            ["id"] = Legalize,
            ["plugInId"] = AutoLegality,
            ["label"] = "Legalize",
            ["description"] = "Adds a button to legalize a pokemon when editing it.",
            ["disabled"] = false,
            ["reason"] = null,
        }.ToJsonString());
    }

    [Fact]
    public void LegalizeMakesAnIllegalPokemonLegal()
    {
        var (session, _, _) = Hosted();
        var (at, _) = IllegalPikachu(session);

        var outcome = Value(Dispatch(session, "plugins.run", Call(Legalize, at)))!;

        var saved = session.Game!.SaveFile.GetPartySlotAtIndex(at.Slot);
        new LegalityAnalysis(saved).Valid.Should().BeTrue();
        saved.CurrentLevel.Should().Be(50);
        outcome["message"]!.GetValue<string>().Should().Be("Applied legality");
    }

    [Fact]
    public void ItsSettingsConfigureAutoLegalityModeBeforeEachRun()
    {
        var (session, host, _) = Hosted();
        host.UpdateSetting(AutoLegality, Timeout, new Settings.SettingValue.IntegerValue(7));
        host.UpdateSetting(AutoLegality, ForceLevel100, new Settings.SettingValue.BooleanValue(true));
        APILegality.Timeout = 1;
        APILegality.ForceLevel100for50 = false;

        Value(Dispatch(session, "plugins.run", Call(Legalize, PokemonHandle.Party(0))));

        APILegality.Timeout.Should().Be(7);
        APILegality.ForceLevel100for50.Should().BeTrue();
    }

    [Fact]
    public async Task LegalizesOnChangeAndOnSaveWhenToggledOn()
    {
        var (session, host, ran) = Hosted();
        host.SetToggle(AutoLegality, LegalizeOnChange, true);
        host.SetToggle(AutoLegality, LegalizeOnSave, true);

        var (_, pikachu) = IllegalPikachu(session);
        var changed = pikachu.Clone();
        var saved = pikachu.Clone();

        await host.PokemonChanged(changed);
        changed.Legality().Valid.Should().BeTrue();

        await host.PokemonSaved(saved);
        saved.Legality().Valid.Should().BeTrue();

        ran.Select(r => r.HookId).Should().Equal(LegalizeOnChange, LegalizeOnSave);
    }

    [Fact]
    public async Task DoesNothingOnChangeOrOnSaveWhenToggledOff()
    {
        var (session, host, ran) = Hosted();
        host.SetToggle(AutoLegality, LegalizeOnChange, false);
        host.SetToggle(AutoLegality, LegalizeOnSave, false);
        var (_, pikachu) = IllegalPikachu(session);

        await host.PokemonChanged(pikachu);
        await host.PokemonSaved(pikachu);

        pikachu.Legality().Valid.Should().BeFalse();
        ran.Should().BeEmpty();
    }
}
