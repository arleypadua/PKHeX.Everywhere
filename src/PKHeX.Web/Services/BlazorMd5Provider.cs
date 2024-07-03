using PKHeX.Core;

namespace PKHeX.Web.Services;

public class BlazorMd5Provider(JsService jsService) : IMd5Provider
{
    public void HashData(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        var hash = jsService.Md5Hash(source.ToArray());
        hash.CopyTo(destination);
    }
}