using System.Text.RegularExpressions;
using PKHeX.Core;

namespace PKHeX.CLI.Extensions;

public static partial class SpeciesExtensions
{
    public static string Name(this Species species) => PascalCaseRegex().Replace(species.ToString(), " $1");
    
    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseRegex();
}