using static PKHeX.Web.Plugins.Outcome.PlugInPage.PageLayout;

namespace PKHeX.Web.Plugins.LiveRun;

public class GoToLiveRun(
    LiveRunPlugin settings,
    IGameProvider gameProvider) : IQuickAction
{   
    public string Description => "Adds a button to play the game with the current opened save game.";
    public string Label => "Play with this save";
    
    public IDisable.DisableInfo DisabledInfo => gameProvider.GetDisabled(settings);

    public Task<Outcome> OnActionRequested() => Outcome.Page("live-run", typeof(LiveRun), layout: Empty)
        .Completed();
}