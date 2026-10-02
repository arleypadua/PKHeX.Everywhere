using System.Runtime.InteropServices.JavaScript;

namespace PKHeX.Everywhere.Engine.Host;

// The browser runtime has no AES or MD5, so PKHeX's save crypto calls the module the SDK's wasmHost registers.
internal static partial class JsCrypto
{
    private const string Module = "pkhex-crypto";

    [JSImport("encryptAes", Module)]
    public static partial byte[] EncryptAes(byte[] key, byte[] data, string mode, byte[]? iv);

    [JSImport("decryptAes", Module)]
    public static partial byte[] DecryptAes(byte[] key, byte[] data, string mode, byte[]? iv);

    [JSImport("md5", Module)]
    public static partial byte[] Md5(byte[] data);
}
