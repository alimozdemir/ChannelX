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
using StackExchange.Redis;

namespace ChannelX.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class Chat : Hub
    {
        readonly DatabaseContext _db;
        readonly UserTracker _tracker;
        // readonly IDistributedCache _cache;
        readonly RedisConnection _redis_db;
        readonly IRedisConnectionFactory _fact;
        // public Chat(DatabaseContext db, UserTracker tracker, IDistributedCache cache)
        public Chat(DatabaseContext db, UserTracker tracker, IRedisConnectionFactory fact)
        {
            _db = db;
            _tracker = tracker;
            _fact = fact;
            _redis_db = _fact.Connection();
            
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = _tracker.Remove(Context.Connection);
            
            // last seen is updated ondisconnectedasync
            HashEntry entry = new HashEntry(user.UserId.ToString(), DateTime.Now.ToString());
            HashEntry[] arr = new HashEntry[1];
            arr[0] = entry;
            _redis_db.HashSet("LastSeen" + user.GroupId.ToString(), arr);
            
            #region Read Example from LastSeen
            var data = _redis_db.HashGetAll("LastSeen" + user.GroupId.ToString());
            foreach(var d in data)
            {
                System.Diagnostics.Debug.WriteLine(d.Name, d.Value);
            }
            #endregion
            
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
            var engagedUsers = await _db.ChannelUsers.Include(i => i.User)
                                .Where(i => i.ChannelId.Equals(model.ChannelId)).ToListAsync();

            var userEngage = engagedUsers.FirstOrDefault(i => i.ChannelId.Equals(model.ChannelId) && i.UserId.Equals(userId));
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
                
                // online users
                var users = (await _tracker.All(userDetail.GroupId)).ToList();

                // all users that engaged with this channel
                foreach(var item in engagedUsers){
                    if (!users.Any(i => i.UserId.Equals(item.UserId))) 
                    {
                        users.Add(new UserDetail(string.Empty, 
                                item.User.UserName, 
                                model.ChannelId.ToString(), 
                                item.State == (int)UserStates.Authorize, item.UserId) );
                    }
                }

                await Clients.Client(Context.ConnectionId).InvokeAsync("UserList", users);
                
                await Clients.Group(model.ChannelId.ToString()).InvokeAsync("UserJoined", userDetail);

                System.Diagnostics.Debug.WriteLine("Joining");
               
                System.Diagnostics.Debug.WriteLine(Context.ConnectionId);
                var currentUser = await _tracker.Find(Context.ConnectionId);

                var messages = _redis_db.ListRange(userDetail.GroupId.ToString(),0,-1);
                foreach(var message in messages)
                {
                    TextModel text = JsonConvert.DeserializeObject<TextModel>(message);

                    System.Diagnostics.Debug.WriteLine("Message:");
                    System.Diagnostics.Debug.WriteLine(message);
                    // Burada değişmesi gereken, kullanıcı bilgilerinin redisden gelmesi 
                    System.Diagnostics.Debug.WriteLine(text.Content);
                    await Clients.Client(Context.ConnectionId).InvokeAsync("Receive", text);
                
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
            
            TextModel message = new TextModel { Content = model.Content, User = user, SentTime = DateTime.Now};
            // _cache.SetString("LastMessage", Convert.ToString(message.Content) );
            
            _redis_db.ListRightPush(user.GroupId.ToString(),message.ToString());
            
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