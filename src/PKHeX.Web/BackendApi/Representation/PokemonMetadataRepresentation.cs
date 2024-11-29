using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Web.BackendApi.Representation;

public class PokemonMetadataRepresentation
{
    public Guid Id { get; set; }
    public uint TID { get; set; }
    public uint SID { get; set; }
    public PokemonTypesMetadataRepresentation Types { get; set; } = default!;
    public uint Pid { get; set; }
    public EntityContext Generation { get; set; }
    public GameVersion GameVersion { get; set; }
    public GenderRepresentation Gender { get; set; } = default!;
    public Species Species { get; set; }
    public string Nickname { get; set; } = default!;
    public int Level { get; set; }
    public uint Experience { get; set; }
    public Nature Nature { get; set; }
    public Nature StatNature { get; set; }
    public PokemonFormMetadataRepresentation Form { get; set; } = default!;
    public bool IsShiny { get; set; }
    public Ability Ability { get; set; }
    public PokemonEggMetadataRepresentation Egg { get; set; } = default!;
    public PokemonFlagsMetadataRepresentation Flags { get; set; } = default!;
    public ItemDefinition? HeldItem { get; set; }
    public List<MoveDefinition> Moves { get; set; } = new();
    public PokemonMetConditionsMetadataRepresentation MetConditions { get; set; } = default!;
    public AllPokemonStatusMetadataRepresentation Stats { get; set; } = default!;

    public bool IsPublic { get; set; }
    public bool AllowDownload { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public DateTime LastSyncedAt { get; set; }
}

public record PokemonTypesMetadataRepresentation(
    int Type1,
    int Type2)
{
    public (int Type1, int Type2) Tuple => (Type1, Type2);
}

public record PokemonFormMetadataRepresentation(
    int Id);

public record PokemonEggMetadataRepresentation(
    bool IsEgg,
    int? HatchCounter);

public record PokemonFlagsMetadataRepresentation(
    bool IsInfected,
    bool IsCured);

public record PokemonMetConditionsMetadataRepresentation(
    GameVersion Version,
    string Location,
    DateOnly? Date,
    int Level,
    bool FatefulEncounter);

public record AllPokemonStatusMetadataRepresentation(
    PokemonStatsMetadataRepresentation Base,
    PokemonStatsMetadataRepresentation Iv,
    PokemonStatsMetadataRepresentation Ev,
    PokemonStatsMetadataRepresentation? Av);

public record PokemonStatsMetadataRepresentation(
    int Health,
    int Attack,
    int Defense,
    int SpecialAttack,
    int SpecialDefense,
    int Speed);

public static class PokemonMetadataRepresentationExtensions
{
    public static string GetShareUrl(this PokemonMetadataRepresentation metadata,
        string baseUrl) => $"{baseUrl}s/{metadata.Id}";
}