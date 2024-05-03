using PKHeX.Facade.Repositories;
using PKHeX.Core;

namespace PKHeX.Facade;

public class Game
{
    private readonly SaveFile _saveFile;

    public Game(SaveFile saveFile)
    {
        _saveFile = saveFile;
        ItemRepository = new ItemRepository(saveFile);

        Trainer = new Trainer(this, saveFile);
    }

    public ItemRepository ItemRepository { get; init; }
    public Trainer Trainer { get; init; }

    public byte[] ToByteArray(BinaryExportSetting setting = BinaryExportSetting.None) => _saveFile.Write(setting);
    public SaveFile GetSaveFile() => _saveFile;
}
