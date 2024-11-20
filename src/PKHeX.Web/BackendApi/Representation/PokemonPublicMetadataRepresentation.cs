using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Web.BackendApi.Representation;

public class PokemonPublicMetadataRepresentation
{
    public Guid Id { get; set; }
    public EntityContext Generation { get; set; }
    public GameVersion GameVersion { get; set; }
    public GenderRepresentation Gender { get; set; } = default!;
    public Species Species { get; set; }
    public string Nickname { get; set; } = default!;
    public int Level { get; set; }
    public uint Experience { get; set; }
    public Nature Nature { get; set; }
    public Nature StatNature { get; set; }
    public bool IsShiny { get; set; }
    public Ability Ability { get; set; }
    public ItemDefinition? HeldItem { get; set; }
    public List<MoveDefinition> Moves { get; set; } = [];

    public bool AllowDownload { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}