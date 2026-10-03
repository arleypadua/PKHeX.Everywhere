using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace PKHeX.Everywhere.Engine.PlugIns;

public enum PlugInSdk
{
    None = 0,
    V1 = 1,
    V2 = 2,
    V3 = 3,
}

internal static class PlugInSdkDetector
{
    private const string V1 = "PKHeX.Web.Plugins";
    private const string Everywhere = "PKHeX.Everywhere.PlugIns";

    // SDK 2 shipped without an assembly version, so it's 1.0.0.0. From SDK 3 on the assembly's major is the SDK's.
    private const int FirstVersionedSdk = 3;

    public static string? NameOf(byte[] assembly)
    {
        try
        {
            using var stream = new MemoryStream(assembly, writable: false);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata) return null;

            var metadata = pe.GetMetadataReader();
            return metadata.IsAssembly ? metadata.GetString(metadata.GetAssemblyDefinition().Name) : null;
        }
        catch (BadImageFormatException)
        {
            return null;
        }
    }

    public static PlugInSdk Detect(byte[] assembly)
    {
        try
        {
            using var stream = new MemoryStream(assembly, writable: false);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata) return PlugInSdk.None;

            var metadata = pe.GetMetadataReader();
            var references = metadata.AssemblyReferences
                .Select(metadata.GetAssemblyReference)
                .Select(r => (Name: metadata.GetString(r.Name), r.Version))
                .ToList();

            if (references.Find(r => r.Name == Everywhere) is { Name: not null } sdk)
                return sdk.Version.Major >= FirstVersionedSdk ? PlugInSdk.V3 : PlugInSdk.V2;
            if (references.Exists(r => r.Name == V1)) return PlugInSdk.V1;
            return PlugInSdk.None;
        }
        catch (BadImageFormatException)
        {
            return PlugInSdk.None;
        }
    }
}

public sealed class IncompatiblePlugInException(PlugInSdk sdk)
    : Exception(sdk == PlugInSdk.None
        ? "The assembly isn't a plug-in."
        : $"The plug-in host can't load a plug-in built against SDK {sdk}.")
{
    public PlugInSdk Sdk { get; } = sdk;
}
