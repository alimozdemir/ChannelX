using System;

namespace ChannelX.Data
{
    public class ChannelUser
    {
        public int ChannelId { get; set; }
        public string UserId { get; set; }
        public int State { get; set; }
        public Channel Channel {get; set;}
        public ApplicationUser User {get; set;}
    }

    public enum UserStates
    {
        Joined,
        Kicked,
        Blocked,
        Authorize
    }
}