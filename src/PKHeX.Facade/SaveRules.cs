using PKHeX.Core;

namespace PKHeX.Facade;

// ParseSettings is global. Gen 1 and 2 need the save's cartridge rules, and nothing else sets them, so the defaults come back after.
internal static class SaveRules
{
    public static T Under<T>(SaveFile save, Func<T> run)
    {
        ParseSettings.InitFromSaveFileData(save);
        try
        {
            return run();
        }
        finally
        {
            ParseSettings.ClearActiveTrainer();
            ParseSettings.AllowEraCartGB = false;
            ParseSettings.AllowEraCartGBA = true;
            ParseSettings.AllowEraSwitchGBA = false;
        }
    }
}
