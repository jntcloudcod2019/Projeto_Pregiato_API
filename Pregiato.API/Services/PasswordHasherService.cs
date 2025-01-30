using Org.BouncyCastle.Crypto.Generators;
using Pregiato.API.Interface;

namespace Pregiato.API.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPasswordHash(string password, string storedHash)
        {
            Console.WriteLine($"Hash gerado: {storedHash}");
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

    }
}
