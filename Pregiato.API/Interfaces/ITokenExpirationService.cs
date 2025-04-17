namespace Pregiato.API.Interfaces
{
    public interface ITokenExpirationService
    {
       Task< DateTime> GetExpirationToken(string token);
    }
}
