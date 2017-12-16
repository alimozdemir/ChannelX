using System;
using Xunit;
using ChannelX.Token;
using Moq;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace ChannelX.Tests
{
    public class JwtUnitTest
    {
        Mock<IOptions<Models.Configuration.Tokens>> _settings;
        public JwtUnitTest()
        {
            _settings = new Mock<IOptions<Models.Configuration.Tokens>>();
            _settings.Setup(i => i.Value).Returns(new Models.Configuration.Tokens());    
            _settings.Object.Value.Issuer = "ChannelX";
            _settings.Object.Value.Audience = "ChannelX";
            _settings.Object.Value.Key = "5fdc4141-0815-4fa9-8c69-f25200e1831a";
            _settings.Object.Value.Expires = 5;
        }
        [Fact]
        public void JwtObjectNotNull()
        {
            var token = new JwtSecurityHelper(_settings.Object);
            var tempGuid = Guid.NewGuid().ToString();
            var jwt = token.GetToken(tempGuid);

            Assert.NotNull(jwt);
        }

        [Fact]
        public void GeneratedIssuerCheck()
        {
            var token = new JwtSecurityHelper(_settings.Object);
            var tempGuid = Guid.NewGuid().ToString();
            var jwt = token.GetToken(tempGuid);

            Assert.Equal(jwt.Issuer, "ChannelX");
        }

        [Fact]
        public void GeneratedAudienceCheck()
        {
            var token = new JwtSecurityHelper(_settings.Object);
            var tempGuid = Guid.NewGuid().ToString();
            var jwt = token.GetToken(tempGuid);

            Assert.Contains("ChannelX", jwt.Audiences);
        }


        [Fact]
        public void GeneratedUserIdCheck()
        {
            var token = new JwtSecurityHelper(_settings.Object);
            var tempGuid = Guid.NewGuid().ToString();
            var jwt = token.GetToken(tempGuid);

            var userClaim = jwt.Claims.FirstOrDefault(i => i.Type.Equals(JwtRegisteredClaimNames.Sub));

            Assert.Equal(userClaim.Value, tempGuid);
        }

        [Fact]
        public void TokenStringNotNull()
        {
            var token = new JwtSecurityHelper(_settings.Object);
            var tempGuid = Guid.NewGuid().ToString();
            var jwt = token.GetToken(tempGuid);
            var tokenString = token.GetTokenValue(jwt);

            Assert.NotEmpty(tokenString);
        }


        [Fact]
        public void TokenStringValidation()
        {
            var token = new JwtSecurityHelper(_settings.Object);
            var tempGuid = Guid.NewGuid().ToString();
            var jwt = token.GetToken(tempGuid);
            var tokenString = token.GetTokenValue(jwt);
            var handler = new JwtSecurityTokenHandler();
            var result = handler.ReadJwtToken(tokenString);

            Assert.Equal(_settings.Object.Value.Issuer, result.Issuer);
        }
    }
}