using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Models.Chat;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ChannelX.Models.Trackers;
// using StackExchange.Redis;
// using Microsoft.Extensions.Caching.Redis;
// using Microsoft.Extensions.Caching.Distributed;
using ChannelX.Redis;

namespace ChannelX.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class Chat : Hub
    {
        readonly DatabaseContext _db;
        readonly UserTracker _tracker;
        // readonly IDistributedCache _cache;
        readonly StackExchange.Redis.IDatabase _redis_db;
        Boolean connected;
        // public Chat(DatabaseContext db, UserTracker tracker, IDistributedCache cache)
        public Chat(DatabaseContext db, UserTracker tracker, IRedisConnectionFactory fact)
        {
            _db = db;
            _tracker = tracker;
            // _cache = cache;
            var conn = fact.Connection();
            connected = true;
            if(conn == null)
            {
                connected = false;
            }
            if(connected)
            {
                _redis_db = fact.Connection().GetDatabase();
            }
            
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = _tracker.Remove(Context.Connection);
            var userModel = new UserModel(user.Name, false);
            await Clients.AllExcept(Context.ConnectionId).InvokeAsync("UserLeft", userModel);
            
            await base.OnDisconnectedAsync(exception);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task Join(JoinModel model)
        {
            var userId = Context.User.GetUserId();
            var user = await _db.Users.FindAsync(userId);
            var channel = await _db.Channels.FindAsync(model.ChannelId);

            if(user != null && channel != null)
            {
                await Groups.AddAsync(Context.ConnectionId, model.ChannelId.ToString());

                var userDetail = new UserDetail(Context.ConnectionId, user.UserName, model.ChannelId.ToString());
                var userModel = new UserModel(user.UserName, channel.OwnerId == userId);

                _tracker.Add(Context.Connection, userDetail);
                
                var users = await _tracker.All(userDetail.GroupId);

                await Clients.Client(Context.ConnectionId).InvokeAsync("UserList", users);

                await Clients.AllExcept(Context.ConnectionId).InvokeAsync("UserJoined", userModel);
            }

        }
        
        public async Task Leave()
        {
            var user = _tracker.Remove(Context.Connection);
            await Groups.RemoveAsync(Context.ConnectionId, user.GroupId);

            var userModel = new UserModel(user.Name, false);
            await Clients.AllExcept(Context.ConnectionId).InvokeAsync("UserLeft", userModel);
        } 

        public async Task Send(TextModel model)
        {
            var user = await _tracker.Find(Context.ConnectionId);
            TextModel message = new TextModel { Content = model.Content, User = user.Name, Type = 1 };
            // _cache.SetString("LastMessage", Convert.ToString(message.Content) );
            if(connected)
            {
                _redis_db.StringSet("LastMessage", message.Content);
                _redis_db.ListRightPush(Context.ConnectionId.ToString(),message.Content);
            }
            System.Diagnostics.Debug.WriteLine(model.Content);
            await Clients.AllExcept(Context.ConnectionId).InvokeAsync("Receive", message);
        }
    }
}