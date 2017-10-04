using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Security.Claims;
using System.Collections.Generic;

namespace ChannelX.Token
{
    public class JwtSecurityHelper
    {
        public static string Issuer {get; set;} = "ChannelX";
        public static string Audience {get; set;} = "ChannelX";
        private static int ExpiresDays { get; }  = 5;
        private const string Secret = "5fdc4141-0815-4fa9-8c69-f25200e1831a";

        public static SymmetricSecurityKey Key(string secret = Secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

        // This will require a claim about user
        public static JwtSecurityToken GetToken(string user)
        {
            var claims = new[]
            {
              new Claim(JwtRegisteredClaimNames.Sub, user),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            return new JwtSecurityToken(issuer:Issuer, 
                                        audience: Audience, 
                                        expires:DateTime.UtcNow.AddDays(ExpiresDays),
                                        claims: claims,
                                        signingCredentials:new SigningCredentials(key:Key(), algorithm:SecurityAlgorithms.HmacSha256));

        }
        
        // Get the value of the token
        public static string GetTokenValue(JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}