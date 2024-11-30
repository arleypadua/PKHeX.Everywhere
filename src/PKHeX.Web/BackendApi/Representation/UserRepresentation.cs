namespace PKHeX.Web.BackendApi.Representation;

public class UserRepresentation
{
    public string Id { get; set; } = null!;
    public int SyncQuota { get; set; }
    public bool IsAdmin { get; set; }
}