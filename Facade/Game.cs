using PkHex.CLI.Facade.Repositories;
using PKHeX.Core;

namespace PkHex.CLI.Facade;

public class Game
{
    private readonly SaveFile _saveFile;
    private readonly List<HistoryEntry> _historyLog = new();

    public Game(SaveFile saveFile)
    {
        _saveFile = saveFile;
        ItemRepository = new ItemRepository(saveFile);

        Trainer = new Trainer(this, saveFile);
    }

    public ItemRepository ItemRepository { get; init; }
    public Trainer Trainer { get; init; }
    public HistoryEntry[] HistoryLog => _historyLog.ToArray();

    public byte[] ToByteArray(BinaryExportSetting setting = BinaryExportSetting.None) => _saveFile.Write(setting);
    public SaveFile GetSaveFile() => _saveFile;

    public record HistoryEntry(string Message);
}
