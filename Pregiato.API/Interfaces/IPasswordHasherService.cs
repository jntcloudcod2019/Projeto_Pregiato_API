namespace Pregiato.API.Interface
{
    public interface IPasswordHasherService
    {
       Task<string> CreatePasswordHashAsync(string password);
       Task<string> GenerateRandomPasswordAsync(int length);

    }
}
