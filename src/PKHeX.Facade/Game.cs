using PKHeX.Facade.Repositories;
using PKHeX.Core;

namespace PKHeX.Facade;

public class Game
{
    internal readonly SaveFile SaveFile;

    public Game(SaveFile saveFile)
    {
        SaveFile = saveFile;
        ItemRepository = new ItemRepository(saveFile);
        MoveRepository = new MoveRepository();

        Trainer = new Trainer(this);
    }

    public ItemRepository ItemRepository { get; init; }
    public MoveRepository MoveRepository { get; init; }
    public Trainer Trainer { get; init; }

    public byte[] ToByteArray() => SaveFile.Write(
        setting: SaveFile.Metadata.GetSuggestedFlags(SaveFile.Extension)
    );
}
