using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChannelX.Models.Chat;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChannelX.Models.Trackers
{
    public class UserTracker : ITracker<HubConnectionContext, UserDetail>
    {
        readonly ConcurrentDictionary<HubConnectionContext, UserDetail> _list;
        public UserTracker()
        {
            _list = new ConcurrentDictionary<HubConnectionContext, UserDetail>();
        }
        public event Action<UserDetail> Added;
        public event Action<UserDetail> Removed;

        public void Add(HubConnectionContext context, UserDetail entity)
        {
            _list.TryAdd(context, entity);

            if (Added != null)
                Added(entity);
        }

        public Task<IEnumerable<UserDetail>> All()
        {
            return Task.FromResult(_list.Values.AsEnumerable());
        }

        public Task<IEnumerable<UserDetail>> All(string key)
        {
            return Task.FromResult(_list.Values.Where(i => i.GroupId.Equals(key)).AsEnumerable());
        }

        public Task<UserDetail> Find(string key)
        {
            return Task.FromResult(_list.Values.FirstOrDefault(i => i.ConnectionId == key));
        }

        public UserDetail Remove(HubConnectionContext context)
        {
            if (_list.TryRemove(context, out var entity))
            {
                if (Removed != null)
                    Removed(entity);

                return entity;
            }

            return null;
        }
    }
}