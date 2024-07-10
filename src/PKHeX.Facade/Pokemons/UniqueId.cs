using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public class UniqueId
{
    public uint _pid;
    public Species _species;

    public string Value => $"{_species.ToString().ToLowerInvariant()}{Separator}{_pid}";

    public override bool Equals(object? obj)
    {
        if (obj is not UniqueId id) return false;
        return Value.Equals(id.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
    
    public static UniqueId From(PKM pokemon) => new() { _pid = pokemon.PID, _species = (Species)pokemon.Species };

    public static UniqueId From(Pokemon pokemon) => new() { _pid = pokemon.PID, _species = pokemon.Species };

    public static UniqueId From(string value)
    {
        var split = value.Split(Separator);
        ArgumentOutOfRangeException.ThrowIfNotEqual(split.Length, 2, $"Pokemon ID: {value}");
        return new UniqueId { _pid = uint.Parse(split[1]), _species = Enum.Parse<Species>(split[0], ignoreCase: true) };
    }

    private const string Separator = ":";
}