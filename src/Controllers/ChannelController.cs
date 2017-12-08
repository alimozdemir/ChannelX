using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Email;
using ChannelX.Models;
using ChannelX.Models.Channel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChannelX.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ChannelController : Controller
    {
        readonly DatabaseContext _db;
        public ChannelController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody]CreateModel model)
        {
            var result = new ResultModel();

            if (ModelState.IsValid) // if model is valid with data annotations
            {
                var userId = User.GetUserId();

                Channel entity = new Channel();

                entity.CreatedAt = DateTime.Now;
                entity.EndAt = entity.CreatedAt.AddHours(model.EndAtHours); // for now
                entity.Title = model.Title;
                entity.IsPrivate = model.IsPrivate;
                entity.Password = model.Password;
                entity.OwnerId = userId;
                entity.Hash = Helper.ShortIdentifier();

                _db.Channels.Add(entity);

                var affected = await _db.SaveChangesAsync();

                if (affected == 1) // only one row should affected
                {
                    result.Succeeded = true;
                    result.Message = "Channel is open.";
                    result.Data = GetSharableLink(entity.Hash);
                }
            }

            return Json(result);
        }
        private string GetSharableLink(string hash) => $"{this.Request.Scheme}://{this.Request.Host}/sh/{hash}";
        [HttpGet("[action]")]
        public async Task<IActionResult> Public()
        {
            var list = await _db.Channels
                        .Where(i => i.EndAt > DateTime.Now && !i.IsPrivate)
                        .Select(i => new ListModel()
                        {
                            Id = i.Id,
                            Title = i.Title,
                            EndTime = i.EndAt,
                            Popularity = i.Users.Count
                        })
                        .ToListAsync();

            return Json(list);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Engaged()
        {
            var userId = User.GetUserId();

            var list = await _db.Channels
                        .Where(i => i.EndAt > DateTime.Now && i.Users.Any(u => u.UserId == userId))
                        .Select(i => new ListModel()
                        {
                            Id = i.Id,
                            Title = i.Title,
                            EndTime = i.EndAt,
                            Popularity = i.Users.Count
                        })
                        .ToListAsync();

            return Json(list);
        }

        [NonAction]
        GetModel FillTheModel(Channel c) => new GetModel()
        {
            Id = c.Id,
            Title = c.Title,
            CreatedAt = c.CreatedAt,
            EndAt = c.EndAt,
            Link = GetSharableLink(c.Hash),
            OwnerId = c.OwnerId,
            CurrentUserId = User.GetUserId()
        };

        [HttpPost("[action]")]
        public async Task<IActionResult> Get([FromBody]IdFormModel model)
        {
            var result = new ResultModel();

            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();

                var data = await _db.Channels.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == model.Id);
                if (data != null)
                {
                    // if the user is owner or member of the channel, then let him in
                    if (data.OwnerId == userId || data.Users.Any(i => i.UserId == userId))
                    {
                        var user = data.Users.FirstOrDefault(i => i.UserId.Equals(userId));
                        if (user != null && user.State == (int)UserStates.Blocked)
                        {
                            result.Succeeded = false;
                            result.Message = "You have blocked from this channel.";
                        }
                        else
                        {
                            result.Succeeded = true;
                            result.Data = FillTheModel(data);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(data.Password))
                        {
                            result.Prompt = true;
                            result.Message = "Enter Password";
                        }
                        else
                        {
                            _db.ChannelUsers.Add(new ChannelUser()
                            {
                                UserId = userId,
                                ChannelId = model.Id,
                                State = (int)UserStates.Joined
                            });

                            var affected = await _db.SaveChangesAsync();
                            if (affected == 1)
                            {
                                result.Data = FillTheModel(data);
                                result.Succeeded = true;
                            }
                        }
                    }
                }
            }

            return Json(result);
        }

        // there should be a common function for get functions
        [HttpPost("[action]")]
        public async Task<IActionResult> GetHash([FromBody]IdStringFormModel model)
        {
            var result = new ResultModel();

            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();

                var data = await _db.Channels.Include(i => i.Users).FirstOrDefaultAsync(i => i.Hash == model.Id);
                if (data != null)
                {
                    // if the user is owner or member of the channel, then let him in
                    if (data.OwnerId == userId || data.Users.Any(i => i.UserId == userId))
                    {
                        result.Succeeded = true;
                        result.Data = FillTheModel(data);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(data.Password))
                        {
                            result.Prompt = true;
                            result.Message = "Enter Password";
                        }
                        else
                        {
                            _db.ChannelUsers.Add(new ChannelUser()
                            {
                                UserId = userId,
                                ChannelId = data.Id,
                                State = (int)UserStates.Joined
                            });

                            var affected = await _db.SaveChangesAsync();
                            if (affected == 1)
                            {
                                result.Data = FillTheModel(data);
                                result.Succeeded = true;
                            }
                        }
                    }
                }
            }

            return Json(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> GetWithPassword([FromBody]PasswordFormModel model)
        {
            var result = new ResultModel();

            if (ModelState.IsValid)
            {
                var data = await _db.Channels.FirstOrDefaultAsync(i => i.Id == model.Id && i.Password.Equals(model.Password));

                if (data != null)
                {
                    var userId = User.GetUserId();

                    _db.ChannelUsers.Add(new ChannelUser()
                    {
                        UserId = userId,
                        ChannelId = model.Id,
                        State = (int)UserStates.Joined
                    });

                    var affected = await _db.SaveChangesAsync();
                    if (affected == 1)
                    {
                        result.Data = FillTheModel(data);
                        result.Succeeded = true;
                    }
                }
                else
                {
                    result.Message = "The informations or password is wrong";
                }
            }

            return Json(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> HistoryPage()
        {
            var userId = User.GetUserId();

            var list = await _db.Channels
                        .Include(i => i.Users)
                        .Select(i => new HistoryModel()
                        {
                            Id = i.Id,
                            Title = i.Title,
                            EndAt = i.EndAt,
                            CreatedAt = i.CreatedAt,
                            EngagedUsersName = i.Users.Select(j => j.User.UserName).ToList(),
                        })
                        .ToListAsync();

            return Json(list);
        }

    }
}