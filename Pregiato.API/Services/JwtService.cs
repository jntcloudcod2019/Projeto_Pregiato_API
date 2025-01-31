using Microsoft.AspNetCore.Identity.Data;
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

            // 2. Criar a chave de segurança
            var secretKey = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var key = new SymmetricSecurityKey(secretKey); // Criar uma instância de SymmetricSecurityKey
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); 

            // 3. Criar o token JWT
            var token = new JwtSecurityToken(
                issuer: "PregiatoAPI",
                audience: "PregiatoAPIToken", 
                claims: claims,
                expires: DateTime.Now.AddMinutes(2), 
                signingCredentials: credentials
            );

            // 4. Retornar o token JWT como string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public Task<string> GetUsernameFromTokenAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                return Task.FromResult(usernameClaim?.Value);
            }
            catch
            {
                return Task.FromResult<string>(null);
            }
        }

        public async Task<string> GetAuthenticatedUsernameAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return null;
            }
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            return await GetUsernameFromTokenAsync(token);
        }

    }

}

