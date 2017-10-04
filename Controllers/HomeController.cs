using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChannelX.Controllers
{
    public class HomeController : Controller
    {
        readonly UserManager<Data.ApplicationUser> _userManager;
        public HomeController(UserManager<Data.ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        // An example token generator
        public IActionResult Login()
        {
            var token = Token.JwtSecurityHelper.GetToken("deneme");
            var key = Token.JwtSecurityHelper.GetTokenValue(token);
            return Json(key);
        }

        // An example authorized action
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Test()
        {

            return Json(new { User = User });
        }

        public async Task<IActionResult> Test2()
        {
            var result = await _userManager.CreateAsync(new Data.ApplicationUser() { UserName = "deneme", Email = "deneme" }, "Deneme123!");

            return Json(result);
        }
    }
}
