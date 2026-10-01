using System.Text.Json.Serialization;

namespace PKHeX.Engine;

public record EngineError(string Code, string Message);

public record Result<T>(bool Ok, T? Value, EngineError? Error)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string code, string message) => new(false, default, new EngineError(code, message));
}

public record SaveSummaryDto(string FileName, string Version, string Trainer);

public record PartySlotDto(int Slot, int SpeciesId, string Species, string Nickname, int Level, bool IsShiny, string HeldItem);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Result<SaveSummaryDto>))]
[JsonSerializable(typeof(Result<PartySlotDto[]>))]
internal partial class EngineJson : JsonSerializerContext;
