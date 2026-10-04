using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// A species the hack defines that PKHeX has no id for. It has data when we have a source for it.
/// </summary>
public sealed record HackSpecies(string Name, HackSpeciesData? Data = null, string? NameInGame = null)
{
    // The game's names fit a nickname's 10 characters, so a longer display name comes with the one the game stores.
    public string DefaultNickname => NameInGame ?? Name;
}

public sealed record HackSpeciesData(
    (MoveType First, MoveType Second) Types,
    (byte HP, byte Attack, byte Defense, byte SpecialAttack, byte SpecialDefense, byte Speed) BaseStats,
    (Ability First, Ability Second, Ability Hidden) Abilities,
    byte GenderRatio,
    GrowthRate Growth,
    byte CatchRate)
{
    internal PersonalInfo Personal => field ??= new PersonalInfo9SV(new byte[PersonalInfo9SV.SIZE])
    {
        HP = BaseStats.HP,
        ATK = BaseStats.Attack,
        DEF = BaseStats.Defense,
        SPA = BaseStats.SpecialAttack,
        SPD = BaseStats.SpecialDefense,
        SPE = BaseStats.Speed,
        Type1 = (byte)Types.First,
        Type2 = (byte)Types.Second,
        // The game falls back to the first ability when the slot the Pokémon uses has none.
        Ability1 = (int)Abilities.First,
        Ability2 = (int)(Abilities.Second == Ability.None ? Abilities.First : Abilities.Second),
        AbilityH = (int)(Abilities.Hidden == Ability.None ? Abilities.First : Abilities.Hidden),
        Gender = GenderRatio,
        EXPGrowth = (byte)Growth,
        CatchRate = CatchRate,
        FormCount = 1,
    };
}

// PKHeX's experience growth rates, in the order of the Gen 3 games' too.
public enum GrowthRate : byte
{
    MediumFast,
    Erratic,
    Fluctuating,
    MediumSlow,
    Fast,
    Slow,
}
