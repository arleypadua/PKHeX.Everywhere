namespace PKHeX.Everywhere.RomHacks.Cfru;

public readonly record struct CfruSpecies(ushort Species, byte Form, bool IsGigantamax = false, uint FormArgument = 0, bool IsEgg = false)
{
    public static implicit operator CfruSpecies((ushort Species, byte Form) national) => new(national.Species, national.Form);

    public bool IsPlain => this == new CfruSpecies(Species, Form);
}
