namespace PKHeX.Web;

public static class Constants
{
    public const string ApplicationName = "PKHeX.Web";
    public static readonly string GitHubRepository = "https://github.com/arleypadua/PKHeX.Everywhere";
    public static readonly string GitHubRepositoryIssues = $"{GitHubRepository}/issues";
    
    public static string GitHubRepositoryNewIssue(string? title = null, string? body = null, string[]? labels = null) => 
        $"{GitHubRepositoryIssues}/new?title={title.Escaped()}&body={body.Escaped()}&labels={labels.Escaped()}";
    
    private static string Escaped(this string? s) => Uri.EscapeDataString(s ?? string.Empty);
    private static string Escaped(this string[]? s) => string.Join(",", s?.Select(v => v.Escaped()) ?? []);
}