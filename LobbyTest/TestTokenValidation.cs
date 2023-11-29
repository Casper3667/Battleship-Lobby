using GameLobby;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LobbyTest
{
    public class Token_Test
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_Validate()
        {
            string token = CreateToken();
            Assert.True(Validate_Token.Validate(token));
        }

        internal string CreateToken()
        {
            string username = "misha";
            string keypass = Validate_Token.key;
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username)
            };

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(keypass));

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken token = new(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}