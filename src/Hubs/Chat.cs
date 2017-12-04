using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            
            await Clients.Group(user.GroupId).InvokeAsync("UserLeft", user);
            
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
            var userEngage = await _db.ChannelUsers.FirstOrDefaultAsync(i => i.ChannelId.Equals(model.ChannelId) && i.UserId.Equals(userId));
            if(user != null && channel != null)
            {
                if(!userId.Equals(channel.OwnerId))
                {
                    if (userEngage != null)
                    {
                        if (userEngage.State == (int)UserStates.Blocked || userEngage.State == (int)UserStates.Kicked)
                        {
                            await Clients.Client(Context.ConnectionId).InvokeAsync("Disconnect");
                            return;
                        }
                    }
                    else
                    {
                        await Clients.Client(Context.ConnectionId).InvokeAsync("Disconnect");
                    }
                }

                await Groups.AddAsync(Context.ConnectionId, model.ChannelId.ToString());

                var userDetail = new UserDetail(Context.ConnectionId, user.UserName, model.ChannelId.ToString(), 
                                        channel.OwnerId == userId, userId);

                _tracker.Add(Context.Connection, userDetail);
                
                var users = await _tracker.All(userDetail.GroupId);

                await Clients.Client(Context.ConnectionId).InvokeAsync("UserList", users);
                
                await Clients.Group(model.ChannelId.ToString()).InvokeAsync("UserJoined", userDetail);

                System.Diagnostics.Debug.WriteLine("Joining");
                if(connected)
                {
                    System.Diagnostics.Debug.WriteLine(Context.ConnectionId);
                    // *** ALIM
                    var currentUser = await _tracker.Find(Context.ConnectionId);

                    var messages = _redis_db.ListRange(userDetail.GroupId.ToString(),0,-1);
                    foreach(var message in messages)
                    {
                        System.Diagnostics.Debug.WriteLine("Message:");
                        System.Diagnostics.Debug.WriteLine(message);
                        // Burada değişmesi gereken, kullanıcı bilgilerinin redisden gelmesi 
                        TextModel text = new TextModel { Content = message, User = currentUser };
                        System.Diagnostics.Debug.WriteLine(text.Content);
                        await Clients.Client(Context.ConnectionId).InvokeAsync("Receive", text);
                    }
                }
            }
        }
        
        public async Task Leave()
        {
            var user = _tracker.Remove(Context.Connection);
            await Groups.RemoveAsync(Context.ConnectionId, user.GroupId);

            await Clients.Group(user.GroupId).InvokeAsync("UserLeft", user);
        } 

        public async Task Send(TextModel model)
        {
            var user = await _tracker.Find(Context.ConnectionId);
            
            TextModel message = new TextModel { Content = model.Content, User = user };
            // _cache.SetString("LastMessage", Convert.ToString(message.Content) );
            if(connected)
            {
                _redis_db.StringSet("LastMessage", message.Content);
                _redis_db.ListRightPush(user.GroupId.ToString(),message.Content);
            }
            System.Diagnostics.Debug.WriteLine(model.Content);
            await Clients.Group(user.GroupId).InvokeAsync("Receive", message);
        }
        public async Task Kick(UserDetail target)
        {
            int id = 0;
            if(int.TryParse(target.GroupId, out id))
            {
                var user = await _tracker.Find(Context.ConnectionId);
                var channel = await _db.Channels.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == id);
                var verifyTarget = await _tracker.Find(target.ConnectionId);
                
                if(verifyTarget.Equals(target))
                {
                    if ( channel.OwnerId != target.UserId )
                    {
                        var channelUserDb = await _db.ChannelUsers.FirstOrDefaultAsync(i => i.ChannelId == id && i.UserId.Equals(target.UserId));

                        channelUserDb.State = (int)UserStates.Kicked;

                        await _db.SaveChangesAsync();

                        await Clients.Client(Context.ConnectionId).InvokeAsync("Disconnect");
                    }
                }
            }
        }
        public async Task Block(UserDetail user)
        {

        }
        public async Task Authorize(UserDetail user)
        {

        }
    }
}