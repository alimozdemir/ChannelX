using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Models;
using ChannelX.Models.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChannelX.Controllers
{
    public class AccountController : Controller 
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly Token.JwtSecurityHelper _jwtHelper;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, Token.JwtSecurityHelper jwtHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]AccountViewModel model)
        {
            ResultModel result = new ResultModel();
            
            if (ModelState.IsValid)
            {
                var signResult = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if(signResult.Succeeded)
                {
                    var token = _jwtHelper.GetToken(model.UserName);
                    var key = _jwtHelper.GetTokenValue(token);
                    result.Succeeded = true;
                    result.Data = key;
                }
                else if (signResult.IsNotAllowed)
                    result.Message = "The user is banned.";
                else
                    result.Message = "User informations are wrong.";
            }

            return Json(result);
        }
    }
}