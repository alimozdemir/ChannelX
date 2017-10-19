using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
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
        readonly Token.JwtSecurityHelper _jwtHelper;
        public HomeController(UserManager<Data.ApplicationUser> userManager, Token.JwtSecurityHelper jwtHelper)
        {
            _userManager = userManager;
            _jwtHelper = jwtHelper;
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
            var token = _jwtHelper.GetToken("deneme");
            var key = _jwtHelper.GetTokenValue(token);
            return Json(key);
        }

        // An example authorized action
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Test()
        {
            var claims = User.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier);

            return Json("Auth");
        }

        public async Task<IActionResult> Test2()
        {
            var result = await _userManager.CreateAsync(new Data.ApplicationUser() { UserName = "deneme", Email = "deneme" }, "Deneme123!");

            return Json(result);
        }
    }
}
