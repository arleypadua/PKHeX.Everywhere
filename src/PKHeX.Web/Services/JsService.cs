using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PKHeX.Web.Services;

public class JsService(IJSRuntime js)
{
    private IJSInProcessRuntime SyncJs => js as IJSInProcessRuntime ??
                                          throw new NotSupportedException(
                                              "Requested an in process javascript interop, but none was found");

    public async ValueTask DownloadFile(Stream stream, string fileName)
    {
        using var streamRef = new DotNetStreamReference(stream);
        await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

    public ValueTask ClickOnAsync(ElementReference? element) => js.InvokeVoidAsync("HTMLElement.prototype.click.call",
        element);

    public void EncryptAes(ReadOnlySpan<byte> origin, Span<byte> destination, ReadOnlySpan<byte> key, CipherMode mode)
    {
        var originHex = BitConverter.ToString(origin.ToArray()).Replace("-", "");
        var keyHex = BitConverter.ToString(key.ToArray()).Replace("-", "");

        var encryptedHex = SyncJs.Invoke<string>("encryptAes", keyHex, originHex, mode.ToString().ToLowerInvariant());

        var encryptedBytes = ConvertHexStringToByteArray(encryptedHex);
        encryptedBytes.CopyTo(destination);
    }

    public void DecryptAes(ReadOnlySpan<byte> origin, Span<byte> destination, ReadOnlySpan<byte> key, CipherMode mode)
    {
        var originHex = BitConverter.ToString(origin.ToArray()).Replace("-", "");
        var keyHex = BitConverter.ToString(key.ToArray()).Replace("-", "");

        var decryptedHex = SyncJs.Invoke<string>("decryptAes", keyHex, originHex, mode.ToString().ToLowerInvariant());

        var decryptedBytes = ConvertHexStringToByteArray(decryptedHex);
        decryptedBytes.CopyTo(destination);
    }
    
    private static byte[] ConvertHexStringToByteArray(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }

    public byte[] Md5Hash(byte[] toArray)
    {
        return SyncJs.Invoke<byte[]>("md5Hash", toArray);
    }
}