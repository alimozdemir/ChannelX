using System;

namespace ChannelX.Models.Trackers
{
    public class UserDetail
    {
        public UserDetail(string connectionId, string name, string groupId, bool auth, string userId, int state)
        {
            ConnectionId = connectionId;
            Name = name;
            GroupId = groupId;
            Authorized = auth;
            UserId = userId;
            State = state;
        }
        public string UserId { get; }
        public string ConnectionId { get; }
        public string Name { get; }
        public string GroupId { get; }
        public bool Authorized { get; }
        public int State { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is UserDetail))
                return false;
            else
            {
                var data = obj as UserDetail;
                return data.Authorized == this.Authorized
                        && data.ConnectionId == this.ConnectionId
                        && data.GroupId == this.GroupId
                        && data.Name == this.Name
                        && data.UserId == this.UserId;

            }
        }
    }
}