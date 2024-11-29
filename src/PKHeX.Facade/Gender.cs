using PKHeX.Facade.Base;

namespace PKHeX.Facade;

public sealed class Gender : Enumeration
{
    private Gender(int id, string name, string symbol) : base(id, name) 
    { 
        Symbol = symbol;
    }

    public string Symbol { get; private set; }
    
    public byte ToByte() => Convert.ToByte(Id);

    public static readonly Gender Male = new (0, "Male", "♂");
    public static readonly Gender Female = new (1, "Female", "♀");
    public static readonly Gender Genderless = new(2, "Genderless", "⚲");
    public static readonly Gender Unknown = new (int.MaxValue, "Unknown", "⁇");

    public static Gender FromByte(byte value) => value switch
    {
        0 => Male,
        1 => Female,
        2 => Genderless,
        _ => Unknown
    };
}
