 using Org.BouncyCastle.Crypto.Generators;
using Pregiato.API.Interface;

namespace Pregiato.API.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public async Task< string> CreatePasswordHashAsync(string password)
        {
            return await Task.FromResult( BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12)));
        }

        public async Task<string> GenerateRandomPasswordAsync(int length)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando Senha do usuário... ");

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
          
            var random = new Random();
            
            var password = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return await Task.FromResult(password);
        }

    }
}
