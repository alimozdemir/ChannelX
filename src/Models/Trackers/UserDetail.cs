using System;

namespace ChannelX.Models.Trackers
{
    public class UserDetail
    {
        public UserDetail(string connectionId, string name, string groupId, string userId, int state)
        {
            ConnectionId = connectionId;
            Name = name;
            GroupId = groupId;
            UserId = userId;
            State = state;
        }
        public string UserId { get; }
        public string ConnectionId { get; set; }
        public string Name { get; }
        public string GroupId { get; }
        public int State { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is UserDetail))
                return false;
            else
            {
                var data = obj as UserDetail;
                return data.ConnectionId == this.ConnectionId
                        && data.GroupId == this.GroupId
                        && data.Name == this.Name
                        && data.UserId == this.UserId;

            }
        }
    }
}