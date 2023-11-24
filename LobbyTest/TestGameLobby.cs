using GameLobby;
using k8s;
using k8s.Models;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static GameLobby.Assign_Server;

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

        [Test]
        public void Test_GetServer()
        {
            string expectedResult = "1.2.3.4";
            Mock<Kubernetes> kubernetesClientMock = new();
            Mock<Assign_Server> assignServerMock = new();
            assignServerMock.Setup(a => a.ListNamespacedPod(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new V1PodList
                {
                    Items = new List<V1Pod>
                    {
                    new() { Status = new V1PodStatus { PodIP = "1.2.3.4" } },
                    new() { Status = new V1PodStatus { PodIP = "5.6.7.8" } }
                    }
                });
            GameServerStatus statusthing = new();
            statusthing.ActivePlayers = 42;
            assignServerMock.Setup(a => a.GetStatus(It.IsAny<string>()))
                .Returns(new GameServerStatus { ActivePlayers = 42 });

            string result = assignServerMock.Object.Find_Server();

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.That(expectedResult, Is.EqualTo(result));
        }
    }
}