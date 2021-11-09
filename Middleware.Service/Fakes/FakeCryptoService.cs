using System;
using Middleware.Service.Utilities;

namespace Middleware.Service.Fakes
{
    public class FakeCryptoService : ICryptoService
    {
        public bool AreEqual(string plainText, string hashedText, string salt)
        {
            return true;
        }

        public string GenerateHash(string input, string salt)
        {
            return input;
        }

        public string GenerateSalt()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
