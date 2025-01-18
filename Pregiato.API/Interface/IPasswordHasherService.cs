namespace Pregiato.API.Interface
{
    public interface IPasswordHasherService
    {
        string CreatePasswordHash(string password);
        bool VerifyPasswordHash(string password, string storedHash);
    }
}
