using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks;

internal static class Gen3SaveSize
{
    private const int Size = 0x20000;

    private static readonly SaveHandlerFooterRTC ClockFooter = new();

    // The handler also accepts Gen 1 and 2 sizes with a footer, so the base size is checked first.
    public static bool IsSupported(int length) =>
        length == Size || ((length & ~0x3F) == Size && ClockFooter.IsRecognized(length));
}
