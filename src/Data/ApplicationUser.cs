using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ChannelX.Data
{
    public class ApplicationUser : IdentityUser
    {
        public List<Channel> Channels { get; set; }
        public List<ChannelUser> EngagedChannels { get; set; }

        public string FirstAndLastName { get; set; }
        public string ForgotPasswordKey { get; set; }
    }
}