using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AwesomeAssertions;

namespace PKHeX.Everywhere.Engine.Tests;

public class RomHacksArchitectureTests
{
    private const string RomHacks = "PKHeX.Everywhere.RomHacks";
    private const string Host = "PKHeX.Everywhere.Engine.Host";

    private static bool IsExempt(string name) =>
        name == RomHacks || name == Host || name.Contains(".Tests") || name.EndsWith(".E2E");

    [Fact]
    public void OnlyTheHostAndTestsReferenceTheRomHacksProject()
    {
        var src = Path.Combine(RepositoryRoot(), "src");

        var referencing = Directory.EnumerateFiles(src, "*.csproj", SearchOption.AllDirectories)
            .Where(project => !IsExempt(Path.GetFileNameWithoutExtension(project)))
            .Where(project => File.ReadAllText(project).Contains($"{RomHacks}.csproj"))
            .Select(Path.GetFileNameWithoutExtension);

        referencing.Should().BeEmpty();
    }

    [Fact]
    public void NoBuiltAssemblyOutsideRomHacksReferencesIt()
    {
        var assemblies = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll", SearchOption.AllDirectories)
            .Where(path => !IsExempt(Path.GetFileNameWithoutExtension(path)))
            .ToList();

        assemblies.Should().Contain(path => Path.GetFileNameWithoutExtension(path) == "PKHeX.Facade")
            .And.Contain(path => Path.GetFileNameWithoutExtension(path) == "PKHeX.Everywhere.Engine");

        assemblies.Where(ReferencesRomHacks).Select(Path.GetFileName).Should().BeEmpty();
    }

    private static bool ReferencesRomHacks(string path)
    {
        using var pe = new PEReader(File.OpenRead(path));
        if (!pe.HasMetadata) return false;

        var metadata = pe.GetMetadataReader();
        return metadata.AssemblyReferences
            .Any(r => metadata.GetString(metadata.GetAssemblyReference(r).Name) == RomHacks);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PKHeX.CLI.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("PKHeX.CLI.sln not found above the test output.");
    }
}
