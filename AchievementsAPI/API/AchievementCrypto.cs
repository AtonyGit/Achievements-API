using System;
using System.Security.Cryptography;
using System.Text;

namespace AchievementsAPI.API;

/// <summary>
/// Encrypts/decrypts the achievements store. On-disk format:
/// 5-byte magic "ACHV1" + 16-byte IV + 32-byte HMAC-SHA256 (over IV+ciphertext)
/// + AES-256-CBC ciphertext (PKCS7) of the UTF-8 JSON.
/// The keys are fixed; the goal is that the file isn't trivially editable, not secrecy.
/// </summary>
internal static class AchievementCrypto
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("ACHV1");
    private const int IvLength = 16;
    private const int HmacLength = 32;

    private static readonly byte[] AesKey =
        SHA256.HashData(Encoding.UTF8.GetBytes("AchievementsAPI::achievements-store::v1"));
    private static readonly byte[] HmacKey =
        SHA256.HashData(Encoding.UTF8.GetBytes("AchievementsAPI::achievements-mac::v1"));

    public static bool IsEncrypted(byte[] bytes) =>
        bytes.Length >= Magic.Length && bytes.AsSpan(0, Magic.Length).SequenceEqual(Magic);

    public static byte[] Encrypt(string json)
    {
        var plain = Encoding.UTF8.GetBytes(json);
        var iv = RandomNumberGenerator.GetBytes(IvLength);

        byte[] cipher;
        using (var aes = Aes.Create())
        {
            aes.Key = AesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var encryptor = aes.CreateEncryptor();
            cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
        }

        var mac = ComputeMac(iv, cipher);

        var output = new byte[Magic.Length + IvLength + HmacLength + cipher.Length];
        Buffer.BlockCopy(Magic, 0, output, 0, Magic.Length);
        Buffer.BlockCopy(iv, 0, output, Magic.Length, IvLength);
        Buffer.BlockCopy(mac, 0, output, Magic.Length + IvLength, HmacLength);
        Buffer.BlockCopy(cipher, 0, output, Magic.Length + IvLength + HmacLength, cipher.Length);
        return output;
    }

    public static bool TryDecrypt(byte[] bytes, out string json)
    {
        json = null!;
        var headerLength = Magic.Length + IvLength + HmacLength;
        if (!IsEncrypted(bytes) || bytes.Length <= headerLength) return false;

        var iv = bytes[Magic.Length..(Magic.Length + IvLength)];
        var storedMac = bytes[(Magic.Length + IvLength)..headerLength];
        var cipher = bytes[headerLength..];

        if (!CryptographicOperations.FixedTimeEquals(storedMac, ComputeMac(iv, cipher)))
            return false;

        try
        {
            using var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var decryptor = aes.CreateDecryptor();
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            json = Encoding.UTF8.GetString(plain);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ComputeMac(byte[] iv, byte[] cipher)
    {
        using var hmac = new HMACSHA256(HmacKey);
        var combined = new byte[iv.Length + cipher.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(cipher, 0, combined, iv.Length, cipher.Length);
        return hmac.ComputeHash(combined);
    }
}
