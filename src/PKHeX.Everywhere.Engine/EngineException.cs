namespace PKHeX.Everywhere.Engine;

public class EngineException(string code, string message, Exception? inner = null) : Exception(message, inner)
{
    public string Code { get; } = code;
}
