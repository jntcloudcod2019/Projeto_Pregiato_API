using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pregiato.API.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeSpan _tokenExpiry = TimeSpan.FromHours(4);

        private readonly string SECRETKEY_JWT_TOKEN = Environment.GetEnvironmentVariable("SECRETKEY_JWT_TOKEN")?? "3+XcgYxev9TcGXECMBq0ilANarHN68wsDsrhG60icMaACkw9ajU97IYT+cv9IDepqrQjPaj4WUQS3VqOvpmtDw==";
        private readonly string ISSUER_JWT = Environment.GetEnvironmentVariable("ISSUER_JWT") ?? "PregiatoAPI";
        private readonly string AUDIENCE_JWT = Environment.GetEnvironmentVariable("AUDIENCE_JWT") ?? "PregiatoAPIToken";
        public JwtService(IMemoryCache memoryCache, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
           
            _memoryCache = memoryCache;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _secretKey = Environment.GetEnvironmentVariable("SECRETKEY_JWT_TOKEN") ??
                         "3+XcgYxev9TcGXECMBq0ilANarHN68wsDsrhG60icMaACkw9ajU97IYT+cv9IDepqrQjPaj4WUQS3VqOvpmtDw==";
            _issuer = Environment.GetEnvironmentVariable("ISSUER_JWT") ?? "PregiatoAPI";
            _audience = Environment.GetEnvironmentVariable("AUDIENCE_JWT") ?? "PregiatoAPIToken";
        }
        public async Task<string> GenerateToken(LoginUserRequest user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.NickName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                    new Claim(ClaimTypes.Role, user.UserType),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.Add(_tokenExpiry),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _issuer,
                Audience = _audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            return await Task.Run(() =>
            {
                if (_memoryCache.TryGetValue($"blacklisted_{token}", out _))
                    return false;

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_secretKey);

                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = _issuer,
                        ValidateAudience = true,
                        ValidAudience = _audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    }, out _);

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> InvalidateTokenAsync(string token)
        {
            return await Task.Run(() =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return false;

                var jwtToken = tokenHandler.ReadJwtToken(token);
                var remainingLife = jwtToken.ValidTo - DateTime.UtcNow;

                if (remainingLife <= TimeSpan.Zero)
                    return false;

                _memoryCache.Set($"blacklisted_{token}", true, remainingLife);
                return true;
            });
        }

        public async Task<string> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                if (!tokenHandler.CanReadToken(token))
                {
                    throw new SecurityTokenException("Token inválido");
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Busca a claim de forma segura
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
                if (userIdClaim == null)
                {
                    throw new SecurityTokenException("Token não contém o ID do usuário");
                }

                return userIdClaim.Value;
            }
            catch (Exception ex)
            {
              Console.WriteLine( $"Falha ao extrair ID do usuário do token. {ex}");
              throw;
            }
        }

        public async Task<string> GetUsernameFromTokenAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                
                return await Task.FromResult(usernameClaim?.Value);
            }
            catch
            {
                return await Task.FromResult<string>($"Erro ao retornar token do usuário.");
            }
        }

        public async Task<string> GetAuthenticatedUsernameAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext não está disponível.");
            }

            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new InvalidOperationException("Cabeçalho de autorização inválido.");
            }
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            return await GetUsernameFromTokenAsync(token);
        }

    }

}

