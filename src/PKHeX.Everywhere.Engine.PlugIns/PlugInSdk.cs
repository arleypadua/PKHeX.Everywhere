using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace PKHeX.Everywhere.Engine.PlugIns;

public enum PlugInSdk
{
    None = 0,
    V1 = 1,
    V2 = 2,
}

internal static class PlugInSdkDetector
{
    private const string V1 = "PKHeX.Web.Plugins";
    private const string V2 = "PKHeX.Everywhere.PlugIns";

    public static PlugInSdk Detect(byte[] assembly)
    {
        try
        {
            using var stream = new MemoryStream(assembly, writable: false);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata) return PlugInSdk.None;

            var metadata = pe.GetMetadataReader();
            var references = metadata.AssemblyReferences
                .Select(r => metadata.GetString(metadata.GetAssemblyReference(r).Name))
                .ToHashSet();

            if (references.Contains(V2)) return PlugInSdk.V2;
            if (references.Contains(V1)) return PlugInSdk.V1;
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
