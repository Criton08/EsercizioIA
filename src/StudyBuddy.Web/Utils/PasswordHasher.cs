using System.Security.Cryptography;

namespace StudyBuddy.Web.Utils;

// PBKDF2 password hashing. Stored format: "v1.<iterations>.<salt base64>.<hash base64>"
public static class PasswordHasher
{
    private const string Prefix = "v1";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Prefix}.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    // needsRehash is true when the stored value is an old plaintext password: the caller must store a new hash.
    public static bool Verify(string password, string stored, out bool needsRehash)
    {
        needsRehash = false;
        var parts = stored.Split('.');

        if (parts.Length != 4 || parts[0] != Prefix)
        {
            // Legacy plaintext row created before hashing was introduced.
            var a = System.Text.Encoding.UTF8.GetBytes(password);
            var b = System.Text.Encoding.UTF8.GetBytes(stored);
            var ok = CryptographicOperations.FixedTimeEquals(a, b);
            needsRehash = ok;
            return ok;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, int.Parse(parts[1]), HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
