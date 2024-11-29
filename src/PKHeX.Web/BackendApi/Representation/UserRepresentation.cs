namespace PKHeX.Web.BackendApi.Representation;

public class UserRepresentation
{
    public string Id { get; set; }
    public int SyncQuota { get; set; }
    public bool IsAdmin { get; set; }
}