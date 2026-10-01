using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Everywhere.Engine.Tests.PlugIn;

public class TestSettings : Settings
{
    public const string Greeting = "Greeting";
    public const string Locked = "Locked";

    public TestSettings() : base(new PlugInManifest("Test Plug-In", "A plug-in for host tests"))
    {
        this[Greeting] = new SettingValue.StringValue("Hello");
        this[Locked] = new SettingValue.StringValue("fixed", ReadOnly: true);

        EnabledByDefault<Greet>();
        EnabledByDefault<Fail>();
        EnabledByDefault<EchoItem>();
        EnabledByDefault<FailOnItem>();
        EnabledByDefault<RenameOnChange>();
        EnabledByDefault<RenameOnSave>();

        DeclarePage(new PlugInPage("hello", "hello.js", PageLayout.Standard, "Hello"));
    }
}

public class Greet(IGameProvider game, TestSettings settings) : IQuickAction
{
    public string Description => "Greets the trainer";
    public string Label => "Greet";
    public IDisable.DisableInfo DisabledInfo => IDisable.Enabled;

    public Task<Outcome> OnActionRequested() =>
        Outcome.Notify($"{settings.GetString(TestSettings.Greeting)}, {game.LoadedGame.Trainer.Name}").Completed();
}

public class Fail : IQuickAction
{
    public string Description => "Always fails";
    public string Label => "Fail";
    public IDisable.DisableInfo DisabledInfo => IDisable.Enabled;

    public Task<Outcome> OnActionRequested() => throw new InvalidOperationException("Failed on purpose");
}

public class OpenHello : IQuickAction
{
    public string Description => "Opens the hello page";
    public string Label => "Open hello";
    public IDisable.DisableInfo DisabledInfo => IDisable.Enabled;

    public Task<Outcome> OnActionRequested() => Outcome.OpenPage("hello").Completed();
}

public class Awaits : IQuickAction
{
    public static Task? Gate { get; set; }

    public string Description => "Waits for the test to release it";
    public string Label => "Awaits";
    public IDisable.DisableInfo DisabledInfo => IDisable.Enabled;

    public async Task<Outcome> OnActionRequested()
    {
        await Gate!;
        return Outcome.Notify("Awaited");
    }
}

public class Unavailable : IQuickAction
{
    public string Description => "Is never available";
    public string Label => "Unavailable";
    public IDisable.DisableInfo DisabledInfo => IDisable.Disabled();

    public Task<Outcome> OnActionRequested() => Outcome.Notify("Ran anyway").Completed();
}

public class EchoItem : IRunOnItemChanged
{
    public string Description => "Echoes the changed item";

    public Task<Outcome> OnItemChanged(IRunOnItemChanged.ItemChanged item) =>
        Outcome.Notify($"{item.Id} x{item.Count}").Completed();
}

public class FailOnItem : IRunOnItemChanged
{
    public string Description => "Fails on every item change";

    public Task<Outcome> OnItemChanged(IRunOnItemChanged.ItemChanged item) =>
        throw new InvalidOperationException($"Failed on item {item.Id}");
}

public class WriteOnItem : IRunOnItemChanged
{
    public static Action? Write { get; set; }

    public string Description => "Writes the save on every item change";

    public Task<Outcome> OnItemChanged(IRunOnItemChanged.ItemChanged item)
    {
        Write?.Invoke();
        return Outcome.Void.Completed();
    }
}

public class RenameOnChange : IRunOnPokemonChange
{
    public string Description => "Renames a changed Pokémon";

    public Task<Outcome> OnPokemonChange(Pokemon pokemon)
    {
        pokemon.ChangeNickname("Changed");
        return Outcome.Void.Completed();
    }
}

public class RenameOnSave : IRunOnPokemonSave
{
    public string Description => "Renames a saved Pokémon";

    public Task<Outcome> OnPokemonSaved(Pokemon pokemon)
    {
        pokemon.ChangeNickname("Saved");
        return Outcome.Void.Completed();
    }
}

public class LevelUp : IPokemonEditAction
{
    public string Description => "Raises the Pokémon's level by one";
    public string Label => "Level up";

    public Task<Outcome> OnActionRequested(Pokemon pokemon)
    {
        var details = pokemon.Details();
        pokemon.Update(new PokemonPatch(Level: details.Level + 1));
        return Outcome.Notify($"{details.Nickname} is level {details.Level + 1}").Completed();
    }
}
