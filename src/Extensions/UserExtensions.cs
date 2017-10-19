using System;
using System.Linq;
using System.Security.Claims;

namespace ChannelX
{
    public static class UserExtensions 
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier);
            
            return claim != null ? claim.Value : string.Empty;
        }
    }

}