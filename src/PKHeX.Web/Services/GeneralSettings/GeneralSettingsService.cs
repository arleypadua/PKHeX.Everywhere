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

    public string CalculatorUrlOrDefault =>
        CalculatorUrl ?? CalculatorRepository.Smogon.Url;

    private string KeyFor(string name) => $"{GeneralSettingsPrefix}{name}";
    private const string GeneralSettingsPrefix = "__settings__#";
}