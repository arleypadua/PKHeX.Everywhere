namespace PKHeX.CLI.Extensions;

public static class PrimitiveExtensions
{
    public static string ToDisplayEmoji(this bool flag) => flag ? "[green]\u2611[/]" : "\u2610"; // ☑ / ☐
}