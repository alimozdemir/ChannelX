using System;

namespace ChannelX.Models.Trackers
{
    public class UserDetail
    {
        public UserDetail(string connectionId, string name, string groupId)
        {
            ConnectionId = connectionId;
            Name = name;
            GroupId = groupId;
        }
        public string ConnectionId { get; }
        public string Name { get; }
        public string GroupId { get; set; }
    }
}