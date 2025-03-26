using System.IdentityModel.Tokens.Jwt;

namespace Pregiato.API.Services.ServiceModels
{
    public class GetTokenExpiration
    {
       public DateTime GetExpirationToken(string token)
       {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
       }
    }
}
