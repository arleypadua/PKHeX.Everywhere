using System.Security.Cryptography;

namespace PKHeX.CLI.Base;

public static class Hasher
{
    public static string ComputeHashOf(string filePath)
    {
        using var sha512 = SHA512.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha512.ComputeHash(stream);
        return hash.ToHashString();
    }

    public static string ComputeHashOf(byte[] bytes)
    {
        using var sha512 = SHA512.Create();
        var hash = sha512.ComputeHash(bytes);
        return hash.ToHashString();
    }
    
    public static bool IsMatch(string file, byte[] bytes) => ComputeHashOf(file) == ComputeHashOf(bytes);

    private static string ToHashString(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }
}