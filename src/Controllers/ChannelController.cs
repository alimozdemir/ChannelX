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

namespace ChannelX.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChannelController : Controller
    {
        readonly DatabaseContext _db;
        public ChannelController(DatabaseContext db)
        {
            _db = db;
        }

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
                }
            }

            return Json(result);
        }
    }
}