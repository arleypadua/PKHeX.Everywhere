using PKHeX.Core;

namespace PKHeX.Everywhere.Engine.Host;

internal sealed class JsMd5Provider : IMd5Provider
{
    public void HashData(ReadOnlySpan<byte> source, Span<byte> destination) => JsCrypto.Md5(source.ToArray()).CopyTo(destination);
}
