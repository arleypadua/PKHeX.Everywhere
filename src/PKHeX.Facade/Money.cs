using PKHeX.Core;

namespace PKHeX.Facade;

public class Money
{
    private readonly Game _game;

    public Money(Game game)
    {
        _game = game;
    }

    public bool IsSupported => _game.SaveFile is not SAV9ZA za
        || (za.Blocks.TryGetBlock(SaveBlockAccessor9ZA.KMoney, out var block) && block.Type != SCTypeCode.None);

    public uint Amount
    {
        get
        {
            try
            {
                return _game.SaveFile.Money;
            }
            catch
            {
                return 0u;
            }
        }
    }

    public void Set(uint amount)
    {
        if (!IsSupported) return;
        _game.SaveFile.Money = (uint)Math.Min(amount, _game.SaveFile.MaxMoney);
    }

    public void SetMax()
    {
        if (!IsSupported) return;
        _game.SaveFile.Money = Convert.ToUInt32(_game.SaveFile.MaxMoney);
    }

    public override string ToString() => Amount.ToString("C");
}
