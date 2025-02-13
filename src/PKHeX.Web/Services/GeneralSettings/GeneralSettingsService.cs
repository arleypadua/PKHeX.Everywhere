using Blazored.LocalStorage;

namespace PKHeX.Web.Services.GeneralSettings;

public class GeneralSettingsService(
    ISyncLocalStorageService localStorage)
{
    public string? CalculatorUrl
    {
        get => localStorage.GetItemAsString(KeyFor("calculatorUrl"));
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                localStorage.RemoveItem(KeyFor("calculatorUrl"));
            }
            else
            {
                var sanitized = value.EndsWith("/") ? value[..^1] : value;
                localStorage.SetItemAsString(KeyFor("calculatorUrl"), sanitized);
            }
        }
    }

    public DateOnly? LastDateNewsSeen
    {
        get
        {
            if (!localStorage.ContainKey("lastDateNewsSeen")) return null;
            return DateOnly.ParseExact(
                localStorage.GetItemAsString("lastDateNewsSeen")!,
                "yyyy-MM-dd");
        }
        set
        {
            if (value == null) localStorage.RemoveItem(KeyFor("lastDateNewsSeen"));
            else localStorage.SetItemAsString("lastDateNewsSeen", value.Value.ToString("yyyy-MM-dd"));
        }
    }

    public string CalculatorUrlOrDefault =>
        CalculatorUrl ?? CalculatorRepository.Smogon.Url;

    private string KeyFor(string name) => $"{GeneralSettingsPrefix}{name}";
    private const string GeneralSettingsPrefix = "__settings__#";
}