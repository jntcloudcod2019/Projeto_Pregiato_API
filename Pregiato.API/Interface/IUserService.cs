namespace Pregiato.API.Interface
{
    public interface IUserService
    {
        Task<string> RegisterUserAsync(string username, string email, string password);
        Task<string> AuthenticateUserAsync(string username, string password);
        Task DeleteUserAsync(Guid id);
    }
}
