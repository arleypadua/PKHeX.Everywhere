namespace PKHeX.Web.Services.GeneralSettings;

public class CalculatorRepository
{
    public static readonly Calculator Smogon = new("Smogon Calculator",
        "Pokemon Showdown default calculator", "https://calc.pokemonshowdown.com");
    
    private static readonly List<Calculator> Calculators = new()
    {
        Smogon,
        new Calculator("KinglerChamp Nuzlocke Calculator", "Calculator with a database of all main line games included",
            "https://kinglerchamp.github.io/VanillaNuzlockeCalc")
    };

    private CalculatorRepository()
    {
    }

    public List<Calculator> GetAll() => Calculators;


    public static CalculatorRepository Instance = new();

    public record Calculator(string Name, string Description, string Url);
}