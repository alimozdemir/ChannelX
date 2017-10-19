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
                Channel entity = new Channel();

                entity.CreatedAt = DateTime.Now;
                entity.EndAt = entity.CreatedAt.AddHours(model.EndAtHours); // for now
                entity.Title = model.Title;
                entity.IsPrivate = model.IsPrivate;
                entity.Password = model.Password;

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
                        .Select(i => new ListModel() {
                            Id = i.Id, 
                            Title = i.Title, 
                            EndTime = i.EndAt, 
                            Popularity = i.Users.Count })
                        .ToListAsync();

            return Json(list);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Engaged()
        {
            var userId = User.GetUserId();
            
            var list = await _db.Channels
                        .Where(i => i.EndAt > DateTime.Now && i.Users.Any(u => u.UserId == userId))
                        .Select(i => new ListModel() {
                            Id = i.Id, 
                            Title = i.Title, 
                            EndTime = i.EndAt, 
                            Popularity = i.Users.Count })
                        .ToListAsync();

            return Json(list);
        }
    }
}