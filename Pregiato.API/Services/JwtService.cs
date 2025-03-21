using Microsoft.IdentityModel.Tokens;
using Pregiato.API.Interface;
using Pregiato.API.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pregiato.API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IModelRepository _modelRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string SECRETKEY_JWT_TOKEN = Environment.GetEnvironmentVariable("SECRETKEY_JWT_TOKEN");
        private readonly string ISSUER_JWT = Environment.GetEnvironmentVariable("ISSUER_JWT");
        private readonly string AUDIENCE_JWT = Environment.GetEnvironmentVariable("AUDIENCE_JWT");
        public JwtService(IConfiguration configuration, IModelRepository modelRepository, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _modelRepository = modelRepository;
            _modelRepository = _modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _httpContextAccessor = httpContextAccessor;
        }
        public string GenerateToken(LoginUserRequest loginUserRequest)
        {
            var user = loginUserRequest;
            var claims = new[]
            {
              new Claim(ClaimTypes.Name, loginUserRequest.Username),
              new Claim(ClaimTypes.Role, loginUserRequest.UserType.ToString())
            };

            var secretKey = Encoding.ASCII.GetBytes(SECRETKEY_JWT_TOKEN);
            var key = new SymmetricSecurityKey(secretKey);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: ISSUER_JWT,
                audience: AUDIENCE_JWT,
                claims: claims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
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

