using System.IdentityModel.Tokens.Jwt;

namespace Pregiato.API.Services.ServiceModels
{
    public class GetTokenExpiration
    {
       public DateTime GetExpirationToken(string token)
       {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
       }
    }
}
