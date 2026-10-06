using PKHeX.Core;

namespace PKHeX.Facade.Extensions;

// PKHeX's own edits look species up in its static tables, which miss the species a save defines.
internal static class PkmEdits
{
    public static void ResetNickname(this PKM pkm)
    {
        if (SpeciesName.GetSpeciesNameGeneration(pkm.Species, pkm.Language, pkm.Format).Length > 0) pkm.ClearNickname();
    }

    // PKHeX's EntityPID.GetRandomPID, with the gender ratio from the Pokémon's personal info instead of PKHeX's Gen 5 table, and the Unown letter kept for every Gen 3 game, not just FireRed and LeafGreen.
    public static uint RandomPid(this PKM pkm, Random random) => pkm.RandomPid(random, pkm.Nature, forceShiny: false);

    private static uint RandomPid(this PKM pkm, Random random, Nature nature, bool forceShiny)
    {
        if (!PidHoldsGender(pkm)) return random.Rand32();

        var ratio = pkm.PersonalInfo.Gender;
        var gen34 = pkm.Version.IsGen3() || pkm.Version.IsGen4();
        var abilityBit = gen34 ? 0x0000_0001u : 0x0001_0000u;
        var unown = pkm.Version.IsGen3() && pkm.Species == (int)Species.Unown;
        while (true)
        {
            var pid = random.Rand32();
            if (forceShiny) pid = ShinyUtil.GetShinyPID(pkm.TID16, pkm.SID16, pid, (uint)random.Next(8));
            if (gen34 && pid % 25 != (byte)nature) continue;
            if (unown ? EntityPID.GetUnownForm3(pid) != pkm.Form : (pid & abilityBit) != (pkm.PID & abilityBit)) continue;
            if (PersonalInfo.IsSingleGender(ratio) || EntityGender.GetFromPIDAndRatio(pid, ratio) == pkm.Gender) return pid;
        }
    }

    public static void SetShinyKeepingGender(this PKM pkm, bool shiny)
    {
        if (pkm.Format <= 2 || !PidHoldsGender(pkm))
        {
            pkm.SetIsShiny(shiny);
            return;
        }

        if (pkm.IsShiny == shiny) return;
        do pkm.PID = pkm.RandomPid(Util.Rand);
        while (pkm.IsShiny != shiny);
        if (pkm.Format >= 6) pkm.EncryptionConstant = pkm.PID;
    }

    // PKHeX's PKM.SetPIDNature makes a shiny Pokémon non-shiny.
    public static void SetPidNature(this PKM pkm, Nature nature)
    {
        var shiny = pkm.IsShiny;
        do pkm.PID = pkm.RandomPid(Util.Rand, nature, shiny);
        while (pkm.IsShiny != shiny);
    }

    private static bool PidHoldsGender(PKM pkm) => pkm.Version is not 0 and < GameVersion.X;
}
