namespace PKHeX.Web.Plugins.LiveRun;

public partial class GoToLiveRun(
    LiveRunPlugin settings,
    IGameProvider gameProvider) : IQuickAction
{
    private readonly LiveRunPlugin _plugin = settings;
    private readonly IGameProvider _gameProvider = gameProvider;
    
    public string Description => "Adds a button to play the game with the current opened save game.";
    public string Label => "Play with this save";
    
    public IDisable.DisableInfo DisabledInfo => _gameProvider.GetDisabled(_plugin);

    public Task<Outcome> OnActionRequested() => Outcome.Page("live-run", RenderPage()).Completed();
}