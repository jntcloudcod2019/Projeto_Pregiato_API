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
        private readonly IUserRepository _userRepository;


        public JwtService(IConfiguration configuration, IModelRepository modelRepository, IUserRepository  userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;   
        }

        public string GenerateToken(LoginUserRequest loginUserRequest)
        {

            var user = loginUserRequest;
            var claims = new[]
            {
              new Claim(ClaimTypes.Name, loginUserRequest.Username),      
              new Claim(ClaimTypes.Role, loginUserRequest.UserType.ToString())
            };

            var secretKey = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var key = new SymmetricSecurityKey(secretKey); 
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); 


            var token = new JwtSecurityToken(
                issuer: "PregiatoAPI",
                audience: "PregiatoAPIToken", 
                claims: claims,
                expires: DateTime.Now.AddHours(2), 
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }

}

