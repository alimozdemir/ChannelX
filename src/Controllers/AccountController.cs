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
using Microsoft.EntityFrameworkCore;

namespace ChannelX.Controllers
{    
    [Route("api/[controller]")]
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
        public IActionResult Index()
        {
            return Json("test");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]AccountViewModel model)
        {
            ResultModel result = new ResultModel();
            
            if (ModelState.IsValid)
            {
                var signResult = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if(signResult.Succeeded)
                {
                    var user = await _userManager.Users.FirstOrDefaultAsync(i => i.UserName == model.UserName);

                    var token = _jwtHelper.GetToken(user.Id);
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
        
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            ResultModel result = new ResultModel();
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName,FirstAndLastName=model.FirstAndLastName,Email = model.Email };
                var Registerresult = await _userManager.CreateAsync(user, model.Password);
                if (Registerresult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    result.Succeeded = true;
                }
                else{
                    result.Message = string.Join(',', Registerresult.Errors.Select(i => i.Description));
                }
            }
            else{
                result.Message="Cannot be empty";
            }
            
            return Json(result);
        }
    }
}