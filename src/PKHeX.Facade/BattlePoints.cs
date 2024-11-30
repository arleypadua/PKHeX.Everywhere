using PKHeX.Core;

namespace PKHeX.Facade;

public abstract class BattlePoints
{
    private BattlePoints() { }
    
    public class NotSupported : BattlePoints
    {
        public static NotSupported Instance { get; } = new();
    }

    public class Supported(SaveFile saveFile) : BattlePoints
    {
        public int BattlePoints
        {
            get => saveFile switch
            {
                SAV4 sav4 => sav4.BP,
                SAV6 sav6 => sav6.BP,
                _ => throw new NotSupportedException("This save file does not support Battle Points")
            };
            set => _ = saveFile switch
            {
                SAV4 sav4 => sav4.BP = value,
                SAV6 sav6 => sav6.BP = value,
                _ => throw new NotSupportedException("This save file does not support Battle Points")
            };
        }
    }
    
    public bool IsSupported(out Supported supported)
    {
        if (this is Supported s)
        {
            supported = s;
            return true;
        }
        
        supported = null!;
        return false;
    }
    
    public static BattlePoints GetInstance(SaveFile saveFile) => saveFile switch
    {
        SAV4 or SAV6 => new Supported(saveFile),
        _ => NotSupported.Instance
    };
}