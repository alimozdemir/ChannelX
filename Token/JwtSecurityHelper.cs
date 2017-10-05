using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace ChannelX.Token
{
    public class JwtSecurityHelper
    {
        Models.Configuration.Tokens _token;
        public JwtSecurityHelper(IOptions<Models.Configuration.Tokens> token)
        {
            _token = token.Value;
        }


        public static SymmetricSecurityKey Key(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

        // This will require a claim about user
        public JwtSecurityToken GetToken(string user)
        {
            var claims = new[]
            {
              new Claim(JwtRegisteredClaimNames.Sub, user),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            return new JwtSecurityToken(issuer:_token.Issuer, 
                                        audience: _token.Audience, 
                                        expires:DateTime.UtcNow.AddDays(_token.Expires),
                                        claims: claims,
                                        signingCredentials:new SigningCredentials(key:Key(_token.Key), algorithm:SecurityAlgorithms.HmacSha256));

        }
        
        // Get the value of the token
        public string GetTokenValue(JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}