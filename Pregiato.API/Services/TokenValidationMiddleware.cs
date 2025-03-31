using Pregiato.API.Interfaces;

namespace Pregiato.API.Services;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
    {
        PathString path = context.Request.Path;
        if (path.StartsWithSegments("/api") && !path.StartsWithSegments("/api/register/user/login"))
        {
            string? token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split("Bearer ").Last();

            if (!string.IsNullOrEmpty(token) && !await jwtService.IsTokenValidAsync(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token inválido ou expirado");
                return;
            }
        }

        await _next(context);
    }
}