using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public partial class Pokemon
{
    /// <summary>
    /// Reads one Gen 3 or Gen 4 Pokémon as its game keeps it in memory: encrypted and shuffled, in its party or box layout.
    /// Nothing is written to a save, and the details hold no legality report.
    /// </summary>
    /// <exception cref="UnreadablePokemonException">The bytes or the version can't be read as a Pokémon.</exception>
    public static PokemonDetails Read(byte[] bytes, int version)
    {
        if (version is < 0 or > byte.MaxValue || ((GameVersion)version).Generation is not (3 or 4))
            throw new UnreadablePokemonException(UnreadableReason.UnsupportedVersion, $"Version {version} isn't a Gen 3 or Gen 4 game.");

        var definition = GameVersionRepository.Instance.FindBlank(version)
            ?? throw new UnreadablePokemonException(UnreadableReason.CombinedVersion, $"Version {version} isn't a single game.");

        var game = Game.EmptyOf(definition);
        var pkm = Decrypted(bytes, game.SaveFile.PKMType, definition);

        if (!pkm.ChecksumValid || !pkm.Valid || pkm is PK3 { FlagIsBadEgg: true } || IsBlank(pkm) || !game.SpeciesRepository.Knows(pkm.Species))
            throw new UnreadablePokemonException(UnreadableReason.BadChecksum, "The bytes aren't a Pokémon.");

        return new Pokemon(pkm, game).Details(withLegality: false);
    }

    // The bytes always arrive encrypted, so they're decrypted outright: PKHeX's constructors guess from the data and can skip it.
    private static PKM Decrypted(byte[] bytes, Type type, GameVersionDefinition definition)
    {
        var data = bytes.ToArray();
        PKM pkm;
        if (type == typeof(PK3))
        {
            pkm = RequireLength(new PK3(), data, definition);
            PokeCrypto.Decrypt3(data);
        }
        else if (type == typeof(PK4))
        {
            pkm = RequireLength(new PK4(), data, definition);
            PokeCrypto.Decrypt45(data);
        }
        else
        {
            throw new UnreadablePokemonException(UnreadableReason.UnsupportedVersion, $"{definition.Name} doesn't keep Pokémon as PK3 or PK4.");
        }

        data.CopyTo(pkm.Data);
        return pkm;
    }

    private static PKM RequireLength(PKM pkm, byte[] data, GameVersionDefinition definition) =>
        data.Length == pkm.SIZE_STORED || data.Length == pkm.SIZE_PARTY
            ? pkm
            : throw new UnreadablePokemonException(UnreadableReason.WrongLength, $"{definition.Name} keeps a Pokémon in {pkm.SIZE_STORED} or {pkm.SIZE_PARTY} bytes, got {data.Length}.");
}

/// <summary>
/// Why <see cref="Pokemon.Read"/> couldn't read a Pokémon.
/// </summary>
public enum UnreadableReason
{
    /// <summary>The checksum doesn't match, the species is empty or unknown, or it's a Gen 3 bad egg.</summary>
    BadChecksum,
    CombinedVersion,
    WrongLength,
    UnsupportedVersion,
}

public class UnreadablePokemonException(UnreadableReason reason, string message) : Exception(message)
{
    public UnreadableReason Reason { get; } = reason;
}
