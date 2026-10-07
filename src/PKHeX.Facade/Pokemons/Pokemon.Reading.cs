using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public partial class Pokemon
{
    /// <summary>
    /// Reads one Gen 3, 4 or 5 Pokémon as its game keeps it in memory: encrypted and shuffled, in its party or box layout.
    /// Nothing is written to a save, and the details hold no legality report.
    /// Party bytes read at the level the game stores in them, box bytes at the level their EXP gives.
    /// </summary>
    /// <exception cref="UnreadablePokemonException">The bytes or the version can't be read as a Pokémon.</exception>
    public static PokemonDetails Read(byte[] bytes, int version)
    {
        if (version is < 0 or > byte.MaxValue || ((GameVersion)version).Generation is not (3 or 4 or 5))
            throw new UnreadablePokemonException(UnreadableReason.UnsupportedVersion, $"Version {version} isn't a Gen 3, 4 or 5 game.");

        var definition = GameVersionRepository.Instance.FindBlank(version)
            ?? throw new UnreadablePokemonException(UnreadableReason.CombinedVersion, $"Version {version} isn't a single game.");

        var game = Game.EmptyOf(definition);
        var pkm = Decrypted(bytes, game.SaveFile.PKMType, definition);

        if (!pkm.ChecksumValid || !pkm.Valid || pkm is PK3 { FlagIsBadEgg: true } || IsBlank(pkm) || !game.SpeciesRepository.Knows(pkm.Species))
            throw new UnreadablePokemonException(UnreadableReason.BadChecksum, "The bytes aren't a Pokémon.");

        return ReadDetails(pkm, game, party: bytes.Length == pkm.SIZE_PARTY);
    }

    /// <summary>
    /// Reads one Pokémon as a game in the Save format keeps it in its party in memory, with the format's species, items and moves.
    /// Nothing is written to a save, and the details hold no legality report. The level is the one the game stores in the party bytes.
    /// </summary>
    /// <exception cref="UnreadablePokemonException">The bytes or the version can't be read as a Pokémon of the format.</exception>
    public static PokemonDetails Read(byte[] bytes, int version, ISaveFormat format)
    {
        var game = Game.EmptyOf(format)
            ?? throw new UnreadablePokemonException(UnreadableReason.UnsupportedFormat, $"{format.Name} can't read a Pokémon without a save.");

        if (version != (int)format.BaseGame)
            throw new UnreadablePokemonException(UnreadableReason.NotTheBaseGame, $"{format.Name} runs on version {(int)format.BaseGame}, not {version}.");

        var save = game.SaveFile;
        if (bytes.Length != save.SIZE_PARTY)
            throw new UnreadablePokemonException(UnreadableReason.WrongLength, $"{format.Name} keeps a party Pokémon in {save.SIZE_PARTY} bytes, got {bytes.Length}.");

        var pkm = save.GetPartySlot(bytes);
        if (!pkm.ChecksumValid || !pkm.Valid || IsBlank(pkm))
            throw new UnreadablePokemonException(UnreadableReason.BadChecksum, "The bytes aren't a Pokémon.");

        return ReadDetails(pkm, game, party: true);
    }

    // A randomizer can change a species' growth rate, so the EXP-derived level may not be the one the game shows.
    private static PokemonDetails ReadDetails(PKM pkm, Game game, bool party)
    {
        var details = new Pokemon(pkm, game).Details(withLegality: false);
        return party ? details with { Level = pkm.Stat_Level } : details;
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
        else if (type == typeof(PK5))
        {
            pkm = RequireLength(new PK5(), data, definition);
            PokeCrypto.Decrypt45(data);
        }
        else
        {
            throw new UnreadablePokemonException(UnreadableReason.UnsupportedVersion, $"{definition.Name} doesn't keep Pokémon as PK3, PK4 or PK5.");
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
    NotTheBaseGame,
    UnsupportedFormat,
}

public class UnreadablePokemonException(UnreadableReason reason, string message) : Exception(message)
{
    public UnreadableReason Reason { get; } = reason;
}
