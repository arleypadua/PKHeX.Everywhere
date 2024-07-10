using PKHeX.Facade;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public class GameServiceProxy(GameService service) : IGameProvider
{
    public Game? Game => service.Game;
    public string? FileName => service.FileName;
    public bool IsLoaded => service.IsLoaded;
}