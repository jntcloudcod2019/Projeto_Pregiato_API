namespace Pregiato.API.Interfaces
{
    public interface ITokenValidationMiddleware
    {
        Task InvokeAsync(HttpContext context);
    }
}
