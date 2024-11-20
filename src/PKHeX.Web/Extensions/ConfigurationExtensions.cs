namespace PKHeX.Web.Extensions;

public static class ConfigurationExtensions
{
    public static BackendApiOptions GetBackendApiOptions(this IConfiguration configuration) => configuration
        .GetSection(BackendApiOptions.SectionName)
        .Get<BackendApiOptions>() ?? new BackendApiOptions();
}

public class BackendApiOptions
{
    public bool Enabled { get; init; }
    public string BaseUri { get; init; } = string.Empty;

    public const string SectionName = "BackendApi";
}