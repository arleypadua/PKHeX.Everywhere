using System.Collections.Frozen;
using System.Collections.Immutable;
﻿using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Events;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

public class Game
{
    public readonly SaveFile SaveFile;

    private static readonly FrozenSet<Capability> AllCapabilities = Enum.GetValues<Capability>().ToFrozenSet();

    public Game(SaveFile saveFile, ISaveFormat? format = null)
    {
        SaveFile = saveFile;
        Format = format is null ? null : new SaveFormatDescription(format.Id, format.Name, format.BaseGame);
        Capabilities = format?.Capabilities ?? AllCapabilities;
        GameData = format?.GameData(saveFile) ?? new PKHeXGameData(saveFile);
        SpeciesRepository = new SpeciesRepository(this);
        PokemonRepository = new PokemonRepository(this);
        LocationRepository = new LocationRepository(this);
        ItemRepository = new ItemRepository(GameData);
        Options = new GameOptions(GameData);

        Trainer = new Trainer(this);
        BattlePoints = BattlePoints.GetInstance(saveFile);
        _events = new Lazy<GameEvents?>(() => Supports(Capability.Events) ? GameEvents.For(this) : null);
    }

    public SaveFormatDescription? Format { get; }

    public IReadOnlySet<Capability> Capabilities { get; }

    public IGameDataSource GameData { get; }

    public bool Supports(Capability capability) => Capabilities.Contains(capability);

    public void Require(Capability capability)
    {
        if (!Supports(capability)) throw new CapabilityNotSupportedException(capability);
    }

    private readonly Lazy<GameEvents?> _events;

    public SpeciesRepository SpeciesRepository { get; }
    public PokemonRepository PokemonRepository { get; }
    public LocationRepository LocationRepository { get; }
    public ItemRepository ItemRepository { get; }
    public GameOptions Options { get; }
    public Trainer Trainer { get; }
    public BattlePoints BattlePoints { get; }
    public GameEvents? Events => _events.Value;
    public Progress Progress => Progress.Of(SaveFile);
    public Badges? Badges => Badges.Of(SaveFile);

    public GameVersionDefinition SaveVersion => GameVersionRepository.Instance.Get(SaveFile.Version);

    /**
     * Sometimes it is not really possible to pinpoint which version a save file is
     * This will give a guess approximation on which actual version the game relates to
     *
     * If the save file yields an actual game version, it will be returned instead of an approximation.
     */
    public GameVersionDefinition GameVersionApproximation => SaveVersion.Aggregated
        ? GameVersionRepository.Instance.Get(SaveFile.Context.GetSingleGameVersion())
        : SaveVersion;

    public EntityContext Generation => SaveFile.Context;

    public IImmutableList<GameVersionDefinition> AvailableVersions =>
        GameVersionRepository.Instance.GetAvailableFor(Generation, SaveVersion.Version);

    public bool IsAwareOf(Species species, byte form = 0) =>
        SaveFile.Personal.IsPresentInGame((ushort)species, form);

    public bool IsAwareOf(Pokemon pokemon) =>
        IsAwareOf(pokemon.Species, pokemon.Pkm.Form);

    public byte[] ToByteArray()
    {
        // make sure pending changes make its way to the bytes of the save
        Trainer.Commit();

        if (SaveFile is SAV7b _7b) _7b.FixStoragePreWrite();

        return SaveFile.Write(
            setting: SaveFile.Metadata.GetSuggestedFlags(Path.GetExtension(SaveFile.Metadata.FileName))
        ).ToArray();
    }

    public static Game LoadFrom(string path) =>
        LoadFrom(() => SaveFormats.Detect(File.ReadAllBytes(path)) ?? FromPKHeXDetection(SaveUtil.GetSaveFile(path)), path);

    public static Game LoadFrom(byte[] bytes, string? path = null, ISaveFormat? format = null) =>
        LoadFrom(() => format switch
        {
            null => SaveFormats.Detect(bytes) ?? FromPKHeXDetection(SaveUtil.GetSaveFile(bytes, path)),
            SaveFormats.PKHeXFormat => FromPKHeX(SaveUtil.GetSaveFile(bytes, path)),
            _ => new Game(format.Load(bytes), format),
        }, path);

    private static Game? FromPKHeXDetection(SaveFile? saveFile) =>
        saveFile is SAV3 gen3 && Gen3Limits.AreExceededBy(gen3) ? null : FromPKHeX(saveFile);

    private static Game? FromPKHeX(SaveFile? saveFile) => saveFile is null ? null : new Game(saveFile);

    /**
     * A save format is picked by file size before anything is parsed, so a file that merely matches a
     * known size is decoded as if it were the real thing. Its contents then fail in whatever way the
     * garbage happens to break first - an unknown species id out of a repository, an offset past the
     * end of a buffer, and so on.
     *
     * All of those mean the same thing to a caller: this file is not a save we can load. Reporting them
     * as GameNotLoadedException gives callers one failure to handle, and keeps the original cause as
     * the inner exception so a real decoding bug is still diagnosable.
     */
    private static Game LoadFrom(Func<Game?> load, string? path)
    {
        try
        {
            return load() ?? throw new GameNotLoadedException(path);
        }
        catch (Exception e) when (e is not (
            GameNotLoadedException or FormatChoiceRequiredException or IOException or UnauthorizedAccessException or OutOfMemoryException))
        {
            throw new GameNotLoadedException(path, e);
        }
    }

    public static Game EmptyOf(
        GameVersionDefinition version,
        string? trainerName = null)
    {
        // PKHeX writes a Gen 2 name one character past the game's limit and overflows, so the Trainer cuts it first.
        var game = new Game(BlankSaveFile.Get(version.Version));
        game.Trainer.Name = trainerName ?? "PKHeXWeb";
        return game;
    }
}

public class GameNotLoadedException(string? path = null, Exception? innerException = null)
    : Exception($"The file {path ?? "N/A"} did not load into a save file.", innerException);