namespace PKHeX.Facade;

public record EntityId(ushort TID, ushort SID)
{
    public override string ToString() => $"{TID}/{SID}";
}