using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GameLobby
{
    public static class Validate_Token
    {
        public static readonly string key = "Thesecretstomakeatokenkeyistodothis";
        public static bool Validate(string JWTToken)
        {
            bool IsValid = ValidateJwt(JWTToken, key);

            return IsValid;
        }

        private static bool ValidateJwt(string jwtToken, string secretKey)
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
