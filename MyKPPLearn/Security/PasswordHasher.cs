using System;
using System.Security.Cryptography;

namespace MyKPPLearn.Security
{
    /// <summary>
    /// Produces and verifies salted PBKDF2 (SHA-256) password hashes.
    /// Encoded format: PBKDF2-SHA256$&lt;iterations&gt;$&lt;saltBase64&gt;$&lt;hashBase64&gt;
    /// This single encoded string is what gets stored in Users.PasswordHash.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Algorithm = "PBKDF2-SHA256";
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] salt = new byte[SaltSizeBytes];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = DeriveKey(password, salt, Iterations, HashSizeBytes);

            return string.Join("$",
                Algorithm,
                Iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            string[] parts = storedHash.Split('$');
            if (parts.Length != 4 || !string.Equals(parts[0], Algorithm, StringComparison.Ordinal))
            {
                return false;
            }

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actualHash = DeriveKey(password, salt, iterations, expectedHash.Length);

            return FixedTimeEquals(actualHash, expectedHash);
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations, int length)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(length);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}
