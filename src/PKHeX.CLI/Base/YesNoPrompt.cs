using Spectre.Console;

namespace PKHeX.CLI.Base;

public static class YesNoPrompt
{
    public static bool AskOrDefault(string questionMarkup, bool defaultValue)
    {
        var answer = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
            .Title(questionMarkup)
            .AddChoices(OptionOrBack.WithValues(
                options: [Yes, No]))
            .WrapAround());

        return answer is OptionOrBack.Option<string> yesNo
            ? yesNo.Value == Yes
            : defaultValue;
    }
    
    public static string LabelFrom(bool value) => value ? Yes : No;
    
    const string Yes = "Yes";
    const string No = "No";
}