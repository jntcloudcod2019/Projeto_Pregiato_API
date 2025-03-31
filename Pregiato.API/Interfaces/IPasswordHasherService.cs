namespace Pregiato.API.Interfaces
{
    public interface IPasswordHasherService
    {
       Task<string> CreatePasswordHashAsync(string password);
       Task<string> GenerateRandomPasswordAsync(int length);

    }
}
