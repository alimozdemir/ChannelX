using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChannelX.Controllers
{
    public class HomeController : Controller
    {
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
            var token = Token.JwtSecurityHelper.GetToken();
            var key = Token.JwtSecurityHelper.GetTokenValue(token);
            return Json(key);
        }

        // An example authorized action
        [Authorize]
        public IActionResult Test()
        {
            return Json("Auth");
        }
    }
}
