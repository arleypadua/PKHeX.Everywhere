namespace PKHeX.Everywhere.Engine;

public static class ErrorCodes
{
    public const string NoSave = "no-save";
    public const string UnknownCall = "unknown-call";
    public const string BadArguments = "bad-arguments";
    public const string Unexpected = "unexpected";
}

public class EngineException(string code, string message, Exception? inner = null) : Exception(message, inner)
{
    public string Code { get; } = code;
}
