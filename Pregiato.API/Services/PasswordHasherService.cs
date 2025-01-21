using Org.BouncyCastle.Crypto.Generators;
using Pregiato.API.Interface;

namespace Pregiato.API.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

    }
}
