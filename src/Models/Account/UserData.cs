using System;

namespace ChannelX.Models.Account
{
    public class UserData
    {
        public UserData(string auth, string userId)
        {
            Auth = auth;
            UserId = userId;
        }
        public string Auth { get; }
        public string UserId { get; }
    }
}