using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace PKHeX.Everywhere.Engine.Tests;

internal static class EngineResults
{
    public static JsonNode? Value(string result)
    {
        var envelope = JsonNode.Parse(result)!;
        envelope["ok"]!.GetValue<bool>().Should().BeTrue(result);
        return envelope["value"];
    }

    public static string Error(string result)
    {
        var envelope = JsonNode.Parse(result)!;
        envelope["ok"]!.GetValue<bool>().Should().BeFalse(result);
        envelope["error"]!["message"]!.GetValue<string>().Should().NotBeNullOrEmpty();
        return envelope["error"]!["code"]!.GetValue<string>();
    }
}
