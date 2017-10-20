using System;
using System.Linq;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Models.Chat;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChannelX.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class Chat : Hub
    {
        readonly DatabaseContext _db;

        public Chat(DatabaseContext db)
        {
            _db = db;
        }
        public Task Join(JoinModel model)
        {
            return Task.FromResult(0);
        }
        public async Task Send(string message)
        {
            var test = _db.Channels.Select(i => new { i.Title, i.Id }).ToListAsync();
            var msg = JsonConvert.SerializeObject(test);
            await Clients.All.InvokeAsync("receive", msg);
        }
    }
}