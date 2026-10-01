using System.Buffers;
using System.Text.Json;

namespace PKHeX.Everywhere.Engine;

public delegate ValueTask<bool> Invoker(Session session, string call, JsonElement args, Utf8JsonWriter writer);

public static class Dispatcher
{
    public static Task<string> Dispatch(Session session, string call, string args) =>
        Dispatch(session, call, args, Invoke);

    private static async ValueTask<bool> Invoke(Session session, string call, JsonElement args, Utf8JsonWriter writer)
    {
        if (await HandlerRegistry.TryInvoke(session, call, args, writer)) return true;
        foreach (var handlers in session.Handlers)
            if (await handlers(session, call, args, writer)) return true;
        return false;
    }

    internal static async Task<string> Dispatch(Session session, string call, string args, Invoker invoke)
    {
        try
        {
            var value = new ArrayBufferWriter<byte>();
            using (var argsDocument = ParseArgs(args))
            using (var writer = new Utf8JsonWriter(value))
            {
                if (!await invoke(session, call, argsDocument.RootElement, writer))
                    throw new EngineException(ErrorCodes.UnknownCall, $"Unknown call '{call}'.");
            }

            return Envelope(w =>
            {
                w.WriteBoolean("ok", true);
                w.WritePropertyName("value");
                w.WriteRawValue(value.WrittenSpan, skipInputValidation: true);
            });
        }
        catch (EngineException e)
        {
            return Failure(e.Code, e.Message);
        }
        catch (Exception e)
        {
            return Failure(ErrorCodes.Unexpected, e.Message);
        }
    }

    private static JsonDocument ParseArgs(string args)
    {
        try
        {
            var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(args) ? "[]" : args);
            if (document.RootElement.ValueKind == JsonValueKind.Array) return document;

            document.Dispose();
        }
        catch (JsonException)
        {
        }

        throw new EngineException(ErrorCodes.BadArguments, "Arguments must be a JSON array.");
    }

    private static string Failure(string code, string message) => Envelope(w =>
    {
        w.WriteBoolean("ok", false);
        w.WriteStartObject("error");
        w.WriteString("code", code);
        w.WriteString("message", message);
        w.WriteEndObject();
    });

    private static string Envelope(Action<Utf8JsonWriter> write)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            write(writer);
            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
    }
}
