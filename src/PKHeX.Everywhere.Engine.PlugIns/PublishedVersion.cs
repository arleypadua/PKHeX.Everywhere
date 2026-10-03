using System.Text.Json;
using System.Text.Json.Serialization;

namespace PKHeX.Everywhere.Engine.PlugIns;

/// <summary>
/// A plug-in version a plug-in source publishes, with the plug-in SDK it targets.
/// </summary>
/// <param name="Version">The version number, such as <c>1.2.0</c>. <c>plugins.newestCompatible</c> skips versions that don't parse as one.</param>
/// <param name="Sdk">The plug-in SDK the version is built against: <c>1</c> for the older PKHeX.Web plug-ins, <c>2</c> and later for PKHeX.Everywhere plug-ins. The engine runs SDK 3 only.</param>
[JsonConverter(typeof(PublishedVersionConverter))]
public sealed record PublishedVersion(string Version, int Sdk);

// Manifests list older versions as plain strings, which target SDK 1.
internal sealed class PublishedVersionConverter : JsonConverter<PublishedVersion>
{
    public override PublishedVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String) return new PublishedVersion(reader.GetString()!, 1);

        var entry = JsonElement.ParseValue(ref reader);
        if (entry.ValueKind != JsonValueKind.Object) throw new JsonException("A published version is a string or an object.");

        string? version = null;
        var sdk = 1;
        foreach (var property in entry.EnumerateObject())
        {
            if (property.NameEquals("Version") || property.NameEquals("version")) version = property.Value.GetString();
            if ((property.NameEquals("Sdk") || property.NameEquals("sdk")) && property.Value.ValueKind == JsonValueKind.Number)
                sdk = property.Value.GetInt32();
        }

        return new PublishedVersion(version ?? throw new JsonException("A published version needs a Version."), sdk);
    }

    public override void Write(Utf8JsonWriter writer, PublishedVersion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Version", value.Version);
        writer.WriteNumber("Sdk", value.Sdk);
        writer.WriteEndObject();
    }
}
