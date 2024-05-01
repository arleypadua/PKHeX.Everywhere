using PKHeX.Core;

namespace PkHex.CLI.Facade;

public class Money
{
    private readonly SaveFile _saveFile;

    public Money(SaveFile saveFile)
    {
        _saveFile = saveFile;
    }

    public decimal Amount => _saveFile.Money;

    public void Set(uint amount)
    {
        _saveFile.Money = amount;
    }

    public void SetMax()
    {
        _saveFile.Money = Convert.ToUInt32(_saveFile.MaxMoney);
    }

    public override string ToString() => Amount.ToString("C");
}