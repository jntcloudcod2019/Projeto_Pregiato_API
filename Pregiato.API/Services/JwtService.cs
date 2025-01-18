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

        public JwtService(IConfiguration configuration, IModelRepository modelRepository)
        {
            _configuration = configuration;
            _modelRepository = modelRepository;
            _modelRepository = _modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));    
        }

        public string GenerateToken(LoginUserRequest loginUserRequest)
        {
            var user = loginUserRequest;
            var claims = new[]
            {
              new Claim(ClaimTypes.Name, loginUserRequest.Username),      
              new Claim(ClaimTypes.Role, loginUserRequest.UserType.ToString()) // ou "Admin", "Manager", etc.
            };

            // 2. Criar a chave de segurança
            var secretKey = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var key = new SymmetricSecurityKey(secretKey); // Criar uma instância de SymmetricSecurityKey
            var credentials = new SigningCredentials(key, SecurityAlgorithms.Sha256); 

            // 3. Criar o token JWT
            var token = new JwtSecurityToken(
                issuer: "SeuIssuer",
                audience: "SuaAudience", 
                claims: claims,
                expires: DateTime.Now.AddMinutes(2), 
                signingCredentials: credentials
            );

            // 4. Retornar o token JWT como string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }

}

