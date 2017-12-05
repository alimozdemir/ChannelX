using System;

namespace ChannelX.Models.Account
{
    public class GetUser
    {
        public GetUser(string userId, string userName, string firstName, string email)
        {
            UserId = userId;
            UserName = userName;
            FirstNameAndLastName = firstName;
            Email = email;
        }
        public string UserId { get; }
        public string UserName { get; }
        public string FirstNameAndLastName { get; }
        public string Email { get; }
    }
}