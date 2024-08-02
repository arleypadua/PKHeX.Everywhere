namespace PKHeX.Facade;

public record EntityId(uint TID, uint SID)
{
    public override string ToString() => $"{TID}/{SID}";
}