using PKHeX.Facade.Base;

namespace PKHeX.Facade;

public sealed class Gender : Enumeration
{
    private Gender(int id, string name, string symbol) : base(id, name) 
    { 
        Symbol = symbol;
    }

    public string Symbol { get; private set; }

    public static readonly Gender Male = new (0, "Male", "♂");
    public static readonly Gender Female = new (1, "Female", "♀");
    public static readonly Gender Genderless = new(2, "Genderless", "⚲");

    public static Gender FromByte(byte value) => value switch
    {
        0 => Male,
        1 => Female,
        2 => Genderless,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} not supported")
    };
}
