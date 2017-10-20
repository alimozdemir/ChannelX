using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Models;
using ChannelX.Models.Channel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
                
                _db.Channels.Add(entity);

                var affected = await _db.SaveChangesAsync();

                if (affected == 1) // only one row should affected
                {
                    result.Succeeded = true;
                    result.Message = "Channel is open.";
                }
            }

            return Json(result);
        }

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
        { Id = c.Id, Title = c.Title, CreatedAt = c.CreatedAt, EndAt = c.EndAt };

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
                                ChannelId = model.Id
                            });

                            var affected = await _db.SaveChangesAsync();
                            if (affected == 1)
                            {
                                var getModel = new GetModel();
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
                        ChannelId = model.Id
                    });

                    var affected = await _db.SaveChangesAsync();
                    if (affected == 1)
                    {
                        var getModel = new GetModel();
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

    }
}