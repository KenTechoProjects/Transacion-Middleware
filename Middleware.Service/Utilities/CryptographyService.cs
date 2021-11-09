using System;
using System.Security.Cryptography;

namespace Middleware.Service.Utilities
{
    public class CryptographyService : ICryptoService
    {
        private const int HASH_LENGTH = 32, ITERATION_COUNT = 128;
        public bool AreEqual(string plainText, string hashedText, string salt)
        {
            return GenerateHash(plainText, salt) == hashedText;
        }

        public string GenerateHash(string input, string salt)
        {
            var generator = new Rfc2898DeriveBytes(input, Convert.FromBase64String(salt), ITERATION_COUNT);
            var hash = generator.GetBytes(HASH_LENGTH);
            return Convert.ToBase64String(hash);
        }

        public string GenerateSalt()
        {
            var salt = new byte[HASH_LENGTH];
            var provider = new RNGCryptoServiceProvider();
            provider.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }
    }
}
