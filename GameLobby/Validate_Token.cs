using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace GameLobby
{
    static public class Validate_Token
    {
        static public readonly string key = "Thesecretstomakeatokenkeyistodothis";
        static public bool Validate(string JWTToken)
        {
            bool IsValid = ValidateJwt(JWTToken, key);

            return IsValid;
        }

        static bool ValidateJwt(string jwtToken, string secretKey)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero 
                };

                // Validate the JWT
                tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);

                return true; // If the validation is successful
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false; // If the validation fails
            }
        }
    }
}
