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
            // when the leave function does not work
            if (user != null)
            {
                // last seen is updated ondisconnectedasync
                _redis_db.UpdateLastSeen(user);
                #region Read Example from LastSeen
                var data = _redis_db.HashGetAll("LastSeen" + user.GroupId.ToString());
                foreach (var d in data)
                {
                    System.Diagnostics.Debug.WriteLine(d.Name, d.Value);
                }
                #endregion

                user.ConnectionId = "";
                await Clients.Group(user.GroupId).InvokeAsync("UserLeft", user);
            }

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
            var channel = await _db.Channels.Include(i => i.Owner)
                                            .Include(i => i.Users)
                                            .ThenInclude(i => i.User)
                                            .FirstOrDefaultAsync(i => i.Id == model.ChannelId);

            var userEngage = await _db.ChannelUsers.FirstOrDefaultAsync(i => i.ChannelId.Equals(model.ChannelId) && i.UserId.Equals(userId));
            if (user != null && channel != null)
            {
                if (!userId.Equals(channel.OwnerId))
                {
                    if (userEngage != null)
                    {
                        if (userEngage.State == (int)UserStates.Blocked)
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
                
                // online users
                var users = (await _tracker.All(model.ChannelId.ToString())).ToList();
                
                if (users.Any(i => i.UserId.Equals(userId)))
                {
                    await Clients.Client(Context.ConnectionId).InvokeAsync("AlreadyConnnected");
                    return;
                }
                
                await Groups.AddAsync(Context.ConnectionId, model.ChannelId.ToString());

                var userDetail = new UserDetail(Context.ConnectionId, user.UserName, model.ChannelId.ToString(), userId,
                                        channel.OwnerId == userId ? (int)UserStates.Authorize : userEngage.State);

                users.Add(userDetail);
                _tracker.Add(Context.Connection, userDetail);
                #region User List
                // load user list
                var engagedUsers = channel.Users
                    .Where(i => i.ChannelId.Equals(model.ChannelId))
                    .Select(i => new UserDetail(string.Empty,
                        i.User.UserName,
                        model.ChannelId.ToString(),
                        i.UserId,
                        i.State)
                    )
                    .ToList();

                // add the owner
                engagedUsers.Add(new UserDetail(string.Empty, channel.Owner.UserName, model.ChannelId.ToString(),
                                        channel.OwnerId,
                                        (int)UserStates.Authorize));


                // if users online take the connection ids
                foreach (var item in users)
                {
                    var temp = engagedUsers.FirstOrDefault(i => i.UserId.Equals(item.UserId));
                    if (temp != null)
                    {
                        temp.ConnectionId = item.ConnectionId;
                    }
                }

                // send user
                await Clients.Client(Context.ConnectionId).InvokeAsync("UserList", engagedUsers);
                #endregion
                // broadcast the user join
                await Clients.Group(model.ChannelId.ToString()).InvokeAsync("UserJoined", userDetail);


                #region Load messages
                var currentUser = await _tracker.Find(Context.ConnectionId);
                var messages = _redis_db.ListRange(userDetail.GroupId.ToString(), 0, -1);
                foreach (var message in messages)
                {
                    TextModel text = JsonConvert.DeserializeObject<TextModel>(message);

                    await Clients.Client(Context.ConnectionId).InvokeAsync("Receive", text);
                }
                #endregion
            }
        }

        public async Task Leave()
        {
            var user = _tracker.Remove(Context.Connection);
            if (user != null)
            {
                // last seen is updated ondisconnectedasync
                _redis_db.UpdateLastSeen(user);
                await Groups.RemoveAsync(Context.ConnectionId, user.GroupId);
                user.ConnectionId = "";
                await Clients.Group(user.GroupId).InvokeAsync("UserLeft", user);
            }
        }

        public async Task Send(TextModel model)
        {
            var user = await _tracker.Find(Context.ConnectionId);

            if (user != null)
            {
                TextModel message = new TextModel { Content = model.Content, User = user, SentTime = DateTime.Now };
                // _cache.SetString("LastMessage", Convert.ToString(message.Content) );

                _redis_db.InsertMessage(user, message);

                System.Diagnostics.Debug.WriteLine(model.Content);
                await Clients.Group(user.GroupId).InvokeAsync("Receive", message);
            }
        }
        public async Task Kick(UserDetail target)
        {
            int id = 0;
            if (int.TryParse(target.GroupId, out id))
            {
                var channel = await _db.Channels.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == id);
                var verifyTarget = await _tracker.Find(target.ConnectionId);

                if (verifyTarget.Equals(target)) // user should be online
                {
                    if (channel.OwnerId != target.UserId)
                    {
                        await Clients.Client(target.ConnectionId).InvokeAsync("Disconnect");

                        target.ConnectionId = "";

                        await UpdateState(target);
                    }
                }
            }
        }
        public async Task Block(UserDetail target)
        {
            int id = 0;
            if (int.TryParse(target.GroupId, out id))
            {
                var user = await _tracker.Find(Context.ConnectionId);
                var channel = await _db.Channels.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == id);
                if (channel.OwnerId != target.UserId)
                {
                    var channelUserDb = await _db.ChannelUsers.FirstOrDefaultAsync(i => i.ChannelId == id && i.UserId.Equals(target.UserId));

                    channelUserDb.State = (int)UserStates.Blocked;
                    target.State = channelUserDb.State;
                    target.ConnectionId = "";

                    await _db.SaveChangesAsync();

                    // if user is online, disconnect him/her.
                    if (!string.IsNullOrEmpty(target.ConnectionId))
                    {
                        await Clients.Client(target.ConnectionId).InvokeAsync("Disconnect");
                    }

                    await UpdateState(target);
                }
            }
        }

        public async Task Authorize(UserDetail target)
        {
            int id = 0;
            if (int.TryParse(target.GroupId, out id))
            {
                var channel = await _db.Channels.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == id);
                if (channel.OwnerId != target.UserId)
                {
                    var channelUserDb = await _db.ChannelUsers.FirstOrDefaultAsync(i => i.ChannelId == id && i.UserId.Equals(target.UserId));

                    channelUserDb.State = (int)UserStates.Authorize;
                    target.State = channelUserDb.State;

                    await _db.SaveChangesAsync();

                    await UpdateState(target);
                }
            }
        }

        public async Task ResetUser(UserDetail target)
        {
            int id = 0;
            if (int.TryParse(target.GroupId, out id))
            {
                var channel = await _db.Channels.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == id);

                if (channel.OwnerId != target.UserId)
                {
                    var channelUserDb = await _db.ChannelUsers.FirstOrDefaultAsync(i => i.ChannelId == id && i.UserId.Equals(target.UserId));

                    channelUserDb.State = (int)UserStates.Joined;
                    target.State = channelUserDb.State;

                    await _db.SaveChangesAsync();

                    await UpdateState(target);

                }
            }
        }

        private async Task UpdateState(UserDetail user)
        {
            await Clients.Group(user.GroupId).InvokeAsync("UpdateState", user);
        }

        public async Task CloseAllWindows()
        {
            var userId = Context.User.GetUserId();
            var users = (await _tracker.All()).Where(i => i.UserId.Equals(userId) 
                                && !i.ConnectionId.Equals(Context.ConnectionId)).ToList();
            
            foreach(var item in users) 
            {
                await Clients.Client(item.ConnectionId).InvokeAsync("Disconnect");
            }
        }
    }
}