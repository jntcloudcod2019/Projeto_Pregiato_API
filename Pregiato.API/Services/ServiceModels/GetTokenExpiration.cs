using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Pregiato.API.Interfaces;

namespace Pregiato.API.Services.ServiceModels
{
    public class GetTokenExpiration : ITokenExpirationService
    {
       public async Task<DateTime> GetExpirationToken(string token)
       {
           try
           {
               var tokenHandler = new JwtSecurityTokenHandler();

               if (!tokenHandler.CanReadToken(token))
               {
                   throw new SecurityTokenException("Token malformado ou ilegível");
               }

               var jwtToken = tokenHandler.ReadJwtToken(token);

               if (jwtToken.ValidTo == DateTime.MinValue)
               {
                   throw new SecurityTokenException("Token sem data de expiração definida");
               }

               return jwtToken.ValidTo;
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Erro ao extrair expiração do token: {ex.Message}");
               throw;
           }
       }
    }
}
