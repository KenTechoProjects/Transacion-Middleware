using System;
using System.Threading.Tasks;

namespace Middleware.Service.Utilities
{
    public interface ICryptoService
    {
        string GenerateSalt();
        string GenerateHash(string input, string salt);
        bool AreEqual(string plainText, string hashedText, string salt);
    }
}
