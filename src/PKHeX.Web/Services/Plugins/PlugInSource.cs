using PKHeX.Everywhere.Engine.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public class PlugInSource
{
    /// <summary>
    /// The url where this source is defined
    /// It is also the identifier of this source
    /// </summary>
    public required string SourceUrl { get; set; }

    public required string Name { get; init; }
    
    /// <summary>
    /// The description of this plug-in source
    /// </summary>
    public string? SourceDescription { get; init; }

    public PlugIn[] PlugIns { get; init; } = [];
    
    public string SourceManifestUrl => $"{SourceUrl.TrimEnd('/')}/{ManifestFileName}";

    public string GetLatestDownloadUrl(PlugIn plugIn) => GetDownloadUrl(plugIn,
        plugIn.NewestCompatibleVersion
        ?? throw new InvalidOperationException($"Plug-in {plugIn.Id} has no version this app can run"));

    public string GetDownloadUrl(PlugIn plugIn, PublishedVersion version)
    {
        if (!PlugIns.Contains(plugIn)) 
            throw new KeyNotFoundException($"Plug-in {plugIn.Id} not found on source {SourceUrl}");

        return $"{SourceUrl.TrimEnd('/')}/{plugIn.Id}/{version.Version}/{plugIn.FileName}";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not PlugInSource other) return false;
        return
            SourceUrl == other.SourceUrl
            && Name == other.Name
            && SourceDescription == other.SourceDescription
            && PlugIns.SequenceEqual(other.PlugIns);
    }

    public override int GetHashCode()
    {
        var plugInsHashCode = PlugIns
            .Aggregate(0, (hash, version) => HashCode.Combine(hash, version.GetHashCode()));
        return HashCode.Combine(SourceUrl, Name, SourceDescription, plugInsHashCode);
    }
    
    public static bool operator ==(PlugInSource? lhs, PlugInSource? rhs)
    {
        if (ReferenceEquals(lhs, rhs)) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(PlugInSource? lhs, PlugInSource? rhs) => !(lhs == rhs);

    public class PlugIn
    {
        public required string Id { get; init; }
        public required string FileName { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required string ProjectUrl { get; init; }
        public string? Summary { get; init; }
        public PublishedVersion[] PublishedVersions { get; init; } = [];

        public PublishedVersion? NewestCompatibleVersion => PlugInHost.NewestCompatible(PublishedVersions);

        public override bool Equals(object? obj)
        {
            if (obj is not PlugIn other) return false;
            return 
                Id == other.Id 
                && FileName == other.FileName 
                && Name == other.Name 
                && Description == other.Description 
                && ProjectUrl == other.ProjectUrl 
                && Summary == other.Summary 
                && PublishedVersions.SequenceEqual(other.PublishedVersions);
        }

        public override int GetHashCode()
        {
            var versionsHashCode = PublishedVersions
                .Aggregate(0, (hash, version) => HashCode.Combine(hash, version.GetHashCode()));
            return HashCode.Combine(Id, FileName, Name, Description, ProjectUrl, Summary, versionsHashCode);
        }
    }

    /// <summary>
    /// A plug-in source is just a folder containing a manifest file with all available plugins
    /// </summary>
    public const string ManifestFileName = "pkhexwebplugins.json";

    /// <summary>
    /// The default path where the default plug-in source is stored
    /// </summary>
    public const string DefaultSourcePath = "https://raw.githubusercontent.com/pkhex-web/plugins-source-assets/main";

    /// <summary>
    /// This is PKHeX web default source of plugins
    /// </summary>
    public static readonly string DefaultSourceUrl = $"{DefaultSourcePath}/{ManifestFileName}";
}