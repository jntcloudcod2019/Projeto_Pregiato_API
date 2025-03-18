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

        public string GenerateRandomPassword(int length)
        {
           
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
          
            var random = new Random();
            
            var password = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return password;
        }

        public bool VerifyPasswordHash(string password, string storedHash)
        {
            Console.WriteLine($"Hash gerado: {storedHash}");
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

    }
}
