using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Services.GeneralSettings;

namespace PKHeX.Web.Services;

public class JsService(IJSRuntime js,
    GeneralSettingsService generalSettings)
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
        var toBeHashedHexString = BitConverter.ToString(toArray.ToArray()).Replace("-", "");
        var hashedHexString = SyncJs.Invoke<string>("md5Hash", toBeHashedHexString);
        return ConvertHexStringToByteArray(hashedHexString);
    }
    
    public Task OpenSmogonDamageCalc(IEnumerable<Pokemon> pokemonList)
    {
        var showdown = pokemonList.Showdown();
        var bytes = Encoding.UTF8.GetBytes(showdown);
        var base64 = Convert.ToBase64String(bytes);
        return OpenNewTab($"{generalSettings.CalculatorUrlOrDefault}/?import={base64}");
    }
    
    public async Task NavigateBack()
    {
        await js.InvokeVoidAsync("history.back");
    }

    public async Task OpenNewTab(string url)
    {
        await js.InvokeVoidAsync("eval", $"window.open('{url}', '_blank')");
    }
}

public static class JsServiceExtensions
{
    public static Task OpenSmogonDamageCalc(this JsService js, Pokemon pokemon) =>
        js.OpenSmogonDamageCalc(new List<Pokemon> { pokemon });
}