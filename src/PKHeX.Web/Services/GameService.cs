using Microsoft.AspNetCore.Components;
using PKHeX.Facade;

namespace PKHeX.Web.Services;

public class GameService
{
    public Game? Game { get; private set; }
    public string? FileName { get; private set; }
    
    public bool IsLoaded => Game != null;

    public event EventHandler? OnGameLoaded;

    public void Load(byte[] bytes, string fileName)
    {
        Game = Game.LoadFrom(bytes, fileName);
        FileName = fileName;

        if (OnGameLoaded is not null)
        {
            OnGameLoaded.Invoke(this, EventArgs.Empty);
        }
    }

    public Stream Export()
    {
        ArgumentNullException.ThrowIfNull(Game, nameof(Game));

        var bytes = Game.ToByteArray();
        return new MemoryStream(bytes);
    }
}