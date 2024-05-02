using Spectre.Console;

namespace PKHeX.CLI.Base;

public enum Result
{
    Continue,
    Exit
}

public static class ResultHandling
{
    public static Result SafeHandle(Func<Result> action)
    {
        try
        {
            return action();
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return Result.Continue;
        }
    }
    
    public static Result RepeatUntilExit(Func<Result> action)
    {
        var result = Result.Continue;
        while (result == Result.Continue)
        {
            result = SafeHandle(action);
        }

        return result;
    }
}
