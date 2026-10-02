using System.Security.Cryptography;
using PKHeX.Core;

namespace PKHeX.Everywhere.Engine.Host;

internal sealed class JsAesProvider : IAesCryptographyProvider
{
    public IAesCryptographyProvider.IAes Create(byte[] key, CipherMode mode, PaddingMode padding, byte[]? iv = null) => new Aes(key, iv);

    private sealed class Aes(byte[] key, byte[]? iv) : IAesCryptographyProvider.IAes
    {
        public void EncryptEcb(ReadOnlySpan<byte> plaintext, Span<byte> destination) =>
            JsCrypto.EncryptAes(key, plaintext.ToArray(), "ecb", null).CopyTo(destination);

        public void DecryptEcb(ReadOnlySpan<byte> ciphertext, Span<byte> destination) =>
            JsCrypto.DecryptAes(key, ciphertext.ToArray(), "ecb", null).CopyTo(destination);

        public void EncryptCbc(ReadOnlySpan<byte> plaintext, Span<byte> destination) =>
            JsCrypto.EncryptAes(key, plaintext.ToArray(), "cbc", iv).CopyTo(destination);

        public void DecryptCbc(ReadOnlySpan<byte> ciphertext, Span<byte> destination) =>
            JsCrypto.DecryptAes(key, ciphertext.ToArray(), "cbc", iv).CopyTo(destination);

        public void Dispose() { }
    }
}
