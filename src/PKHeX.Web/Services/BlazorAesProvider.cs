using PKHeX.Core.Saves.Encryption.Providers;

namespace PKHeX.Web.Services;

public class BlazorAesProvider(JsService jsService) : IAesCryptographyProvider
{
    public IAesCryptographyProvider.IAes Create(byte[] key) => new CryptoJsAes(jsService, key);

    private class CryptoJsAes(JsService jsService, byte[] key) : IAesCryptographyProvider.IAes
    {
        public void EncryptEcb(ReadOnlySpan<byte> origin, Span<byte> destination) =>
            jsService.EncryptEcb(origin, destination, key);

        public void DecryptEcb(ReadOnlySpan<byte> origin, Span<byte> destination) =>
            jsService.DecryptEcb(origin, destination, key);

        public void Dispose()
        {
        }
    }
}