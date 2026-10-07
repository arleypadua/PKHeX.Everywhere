using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Transfers;

internal static class TransferRestore
{
    public static KeptCopy? Keep(Pokemon pokemon, Game to) =>
        to.SaveFile.Generation < pokemon.Pkm.Format && pokemon.IdentityKey is { } key
            ? new KeptCopy(key, pokemon.Pkm.Format, pokemon.ToFile().Bytes)
            : null;

    public static PKM? CopyFor(Pokemon pokemon, Game to, IEnumerable<KeptCopy> copies)
    {
        if (pokemon.IdentityKey is not { } key) return null;

        var applying = copies
            .Where(copy => copy.IdentityKey == key && copy.Generation > pokemon.Pkm.Format && copy.Generation >= to.SaveFile.Generation)
            .OrderBy(copy => copy.Generation);
        foreach (var copy in applying)
        {
            if (Read(copy) is not { } kept || kept.IdentityKey != key || !SameFamily(kept.Pkm, pokemon.Pkm)) continue;
            if (kept.Pkm.GetType() == to.SaveFile.PKMType) return kept.Pkm;
            if (TransferConversion.Convert(kept, to, unofficial: true, out var converted) is null) return converted!.Pkm;
        }

        return null;
    }

    public static void Restore(PKM pk, PKM copy)
    {
        pk.Version = copy.Version;
        pk.MetLevel = copy.MetLevel;
        pk.MetDate = copy.MetDate;
        pk.EggMetDate = copy.EggMetDate;

        // The location and ball setters guess which Gen 4 games wrote them, so the copy's raw values go across instead.
        if (pk is G4PKM g4 && copy is G4PKM copy4)
        {
            (g4.MetLocationDP, g4.MetLocationExtended) = (copy4.MetLocationDP, copy4.MetLocationExtended);
            (g4.EggLocationDP, g4.EggLocationExtended) = (copy4.EggLocationDP, copy4.EggLocationExtended);
            (g4.BallDPPt, g4.BallHGSS) = (copy4.BallDPPt, copy4.BallHGSS);
            g4.ShinyLeaf = copy4.ShinyLeaf;
            g4.WalkingMood = copy4.WalkingMood;
        }
        else
        {
            pk.MetLocation = copy.MetLocation;
            pk.EggLocation = copy.EggLocation;
            pk.Ball = copy.Ball;
        }

        if (pk is IGroundTile tile && copy is IGroundTile copyTile) tile.GroundTile = copyTile.GroundTile;

        if (pk is PK5 pk5 && copy is PK5 copy5)
        {
            pk5.Nature = copy5.Nature;
            pk5.NSparkle = copy5.NSparkle;
            pk5.PokeStarFame = copy5.PokeStarFame;
        }

        if (TransferConversion.CanHaveAbility(pk, copy.Ability))
        {
            pk.Ability = copy.Ability;
            if (pk is PK5 hidden && copy is PK5 { HiddenAbility: var wasHidden })
                hidden.HiddenAbility = wasHidden && hidden.PersonalInfo.GetAbilityAtIndex(2) == copy.Ability;
        }

        AddRibbons(pk, copy);
        pk.RefreshChecksum();
    }

    // A ribbon the Pokémon earned in the older game stays, so the copy's ribbons are added rather than copied over.
    private static void AddRibbons(PKM pk, PKM copy)
    {
        foreach (var ribbon in RibbonInfo.GetRibbonInfo(copy))
        {
            if (ribbon.Type == RibbonValueType.Boolean && ribbon.HasRibbon)
                ReflectUtil.SetValue(pk, ribbon.Name, true);
            else if (ribbon.Type == RibbonValueType.Byte && ribbon.RibbonCount > (byte)ReflectUtil.GetValue(pk, ribbon.Name)!)
                ReflectUtil.SetValue(pk, ribbon.Name, ribbon.RibbonCount);
        }
    }

    private static Pokemon? Read(KeptCopy copy)
    {
        if (copy.Generation is < 3 or > 9) return null;

        var data = copy.Bytes.ToArray();
        var blank = EntityBlank.GetBlank((byte)copy.Generation);
        if (data.Length != blank.SIZE_PARTY && data.Length != blank.SIZE_STORED) return null;

        // Format detection takes some Gen 5 Pokémon for Battle Revolution ones, so Gen 3 to 5 are read as their generation's type.
        var pkm = copy.Generation switch
        {
            3 => new PK3(data),
            4 => new PK4(data),
            5 => new PK5(data),
            _ => EntityFormat.GetFromBytes(data, prefer: (EntityContext)copy.Generation),
        };
        if (pkm is null || pkm.Format != copy.Generation || !pkm.ChecksumValid || Pokemon.IsBlank(pkm)) return null;

        return new Pokemon(pkm, new Game(BlankSaveFile.Get(pkm.Context)));
    }

    // The Pokémon may have evolved in the older game.
    private static bool SameFamily(PKM copy, PKM pokemon) => copy.Species == pokemon.Species || EvolutionTree
        .GetEvolutionTree(copy.Context)
        .GetEvolutionsAndPreEvolutions(copy.Species, copy.Form)
        .Any(evolution => evolution.Species == pokemon.Species);
}
