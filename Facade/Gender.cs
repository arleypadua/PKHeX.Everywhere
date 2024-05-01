using PkHex.CLI.Base;

namespace PkHex.CLI.Facade;

public sealed class Gender : Enumeration
{
    private Gender(int id, string name, string symbol) : base(id, name) 
    { 
        Symbol = symbol;
    }

    public string Symbol { get; private set; }

    public static Gender Male = new (0, "Male", "♂");
    public static Gender Female = new (1, "Female", "♀");

    public static Gender FromByte(byte value) => value switch
    {
        0 => Male,
        1 => Female,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} not supported")
    };
}
