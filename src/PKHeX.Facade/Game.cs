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

        Trainer = new Trainer(this);
    }

    public ItemRepository ItemRepository { get; }
    public Trainer Trainer { get; }

    public byte[] ToByteArray()
    {
        // make sure pending changes make its way to the bytes of the save
        Trainer.Commit();
        
        return SaveFile.Write(
            setting: SaveFile.Metadata.GetSuggestedFlags(Path.GetExtension(SaveFile.Metadata.FileName))
        );
    }

    public static Game LoadFrom(string path)
    {
        var saveFile = SaveUtil.GetVariantSAV(path)
                       ?? throw new InvalidOperationException($"The file at {path} did not load into a save file.");

        return new Game(saveFile);
    }

    public static Game LoadFrom(byte[] bytes, string? path = null)
    {
        var saveFile = SaveUtil.GetVariantSAV(bytes, path)
                       ?? throw new InvalidOperationException($"The file at {path} did not load into a save file.");

        return new Game(saveFile);
    }
}