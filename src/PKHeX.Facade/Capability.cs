namespace PKHeX.Facade;

public enum Capability
{
    Legality,
    AutoLegality,
    Encounters,
    Showdown,
    Events,
    PlugIns,
}

public class CapabilityNotSupportedException(Capability capability)
    : Exception($"This save doesn't support {capability}.")
{
    public Capability Capability { get; } = capability;
}
