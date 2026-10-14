using PKHeX.Core;

namespace PKHeX.Facade;

internal static class LanguageCodes
{
    public static string? Of(int language) => (LanguageID)language switch
    {
        LanguageID.Japanese => "JPN",
        LanguageID.English => "ENG",
        LanguageID.French => "FRE",
        LanguageID.Italian => "ITA",
        LanguageID.German => "GER",
        LanguageID.Spanish => "SPA",
        LanguageID.Korean => "KOR",
        _ => null,
    };
}
