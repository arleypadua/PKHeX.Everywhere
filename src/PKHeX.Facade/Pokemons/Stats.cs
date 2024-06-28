using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public record Stats(PKM Pokemon, Stats.StatsType Type)
{
    public int Attack
    {
        get => Type switch
        {
            StatsType.Base => Pokemon.Stat_ATK,
            StatsType.EV => Pokemon.EV_ATK,
            StatsType.IV => Pokemon.IV_ATK,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
        set => _ = Type switch
        {
            StatsType.Base => Pokemon.Stat_ATK = value,
            StatsType.EV => Pokemon.EV_ATK = value,
            StatsType.IV => Pokemon.IV_ATK = value,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
    }

    public int Defense
    {
        get => Type switch
        {
            StatsType.Base => Pokemon.Stat_DEF,
            StatsType.EV => Pokemon.EV_DEF,
            StatsType.IV => Pokemon.IV_DEF,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
        set => _ = Type switch
        {
            StatsType.Base => Pokemon.Stat_DEF = value,
            StatsType.EV => Pokemon.EV_DEF = value,
            StatsType.IV => Pokemon.IV_DEF = value,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
    }

    public int SpecialAttack
    {
        get => Type switch
        {
            StatsType.Base => Pokemon.Stat_SPA,
            StatsType.EV => Pokemon.EV_SPA,
            StatsType.IV => Pokemon.IV_SPA,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
        set => _ = Type switch
        {
            StatsType.Base => Pokemon.Stat_SPA = value,
            StatsType.EV => Pokemon.EV_SPA = value,
            StatsType.IV => Pokemon.IV_SPA = value,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
    }

    public int SpecialDefense
    {
        get => Type switch
        {
            StatsType.Base => Pokemon.Stat_SPD,
            StatsType.EV => Pokemon.EV_SPD,
            StatsType.IV => Pokemon.IV_SPD,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
        set => _ = Type switch
        {
            StatsType.Base => Pokemon.Stat_SPD = value,
            StatsType.EV => Pokemon.EV_SPD = value,
            StatsType.IV => Pokemon.IV_SPD = value,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
    }

    public int Health
    {
        get => Type switch
        {
            StatsType.Base => Pokemon.Stat_HPMax,
            StatsType.EV => Pokemon.EV_HP,
            StatsType.IV => Pokemon.IV_HP,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
        set => _ = Type switch
        {
            StatsType.Base => Pokemon.Stat_HPMax = value,
            StatsType.EV => Pokemon.EV_HP = value,
            StatsType.IV => Pokemon.IV_HP = value,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
    }

    public int Speed
    {
        get => Type switch
        {
            StatsType.Base => Pokemon.Stat_SPE,
            StatsType.EV => Pokemon.EV_SPE,
            StatsType.IV => Pokemon.IV_SPE,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
        set => _ = Type switch
        {
            StatsType.Base => Pokemon.Stat_SPE = value,
            StatsType.EV => Pokemon.EV_SPE = value,
            StatsType.IV => Pokemon.IV_SPE = value,
            _ => throw new InvalidOperationException($"stats type {Type} not supported"),
        };
    }

    public int? CombatPower
    {
        get
        {
            if (Type is not StatsType.Base) return null;
            return Pokemon is ICombatPower combatPower
                ? combatPower.Stat_CP
                : null;
        }
        set
        {
            if (Type is not StatsType.Base) throw new InvalidOperationException($"stats type {Type} not supported");
            if (Pokemon is ICombatPower combatPower)
            {
                combatPower.Stat_CP = value ?? 0;
            }
        }
    }

    public int Total => Attack + Defense + SpecialAttack + SpecialDefense + Health + Speed;

    public enum StatsType
    {
        Base,
        EV,
        IV
    }

    public static Stats EvFrom(PKM pokemon) => new (pokemon, StatsType.EV);
    public static Stats IvFrom(PKM pokemon) => new (pokemon, StatsType.IV);
    public static Stats BaseFrom(PKM pokemon) => new (pokemon, StatsType.Base);
}