using System.Security.Cryptography;
using System.Text;

namespace ActivaPro.Application.Security
{
    public static class PasswordHasher
    {
        // Formato: PBKDF2$iteraciones$saltBase64$hashBase64
        public static string Hash(string password, int iterations = 100_000)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            try
            {
                var parts = stored.Split('$');
                if (parts.Length != 4 || parts[0] != "PBKDF2") return false;

                int iterations = int.Parse(parts[1]);
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var actualHash = pbkdf2.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
            }
            catch { return false; }
        }
    }
}