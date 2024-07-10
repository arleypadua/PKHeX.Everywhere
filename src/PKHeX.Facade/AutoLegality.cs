using PKHeX.Core.AutoMod;

namespace PKHeX.Facade;

public static class AutoLegality
{
    public static void ApplyDefaultConfiguration(TimeSpan? timeout = null)
    {
        APILegality.ForceLevel100for50 = false;
        APILegality.Timeout = (int)(timeout?.TotalSeconds ?? 15);
    }
}