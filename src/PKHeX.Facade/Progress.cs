using System.Numerics;
using PKHeX.Core;

namespace PKHeX.Facade;

public record PlayTime(int Hours, int Minutes, int Seconds);

public record BadgeCount(int Earned, int Total);

public record PokedexCount(int Seen, int Caught);

public record Progress(PlayTime? PlayTime, BadgeCount? Badges, PokedexCount? Pokedex)
{
    internal static Progress Of(SaveFile save) => new(PlayTimeOf(save), BadgesOf(save), PokedexOf(save));

    private static PlayTime? PlayTimeOf(SaveFile save) => save switch
    {
        SAV_STADIUM or SAV3RSBox or (BulkStorage and not SAV4Ranch) => null,
        _ => new PlayTime(save.PlayedHours, save.PlayedMinutes, save.PlayedSeconds),
    };

    // SWSH keeps a badge byte too, but whether it counts badges or flags them is unconfirmed.
    private static BadgeCount? BadgesOf(SaveFile save) => save switch
    {
        _ when Facade.Badges.Of(save) is { All: var all } => new BadgeCount(all.Count(b => b.Earned), all.Count),
        SAV4HGSS hgss => Earned(hgss.Badges | (hgss.Badges16 << 8), 16),
        SAV4 gen4 => Earned(gen4.Badges, 8),
        SAV5 gen5 => Earned(gen5.Misc.Badges, 8),
        SAV6AODemo => null,
        SAV6 gen6 => Earned(gen6.Badges, 8),
        SAV8BS bdsp => new BadgeCount(bdsp.FlagWork.BadgeCount(), 8),
        _ => null,
    };

    private static BadgeCount Earned(int flags, int total) => new(BitOperations.PopCount((uint)flags), total);

    private static PokedexCount? PokedexOf(SaveFile save) =>
        save.HasPokeDex ? new PokedexCount(save.SeenCount, save.CaughtCount) : null;
}
