using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Email;
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
        readonly IEmailSender _IEmailSender;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, Token.JwtSecurityHelper jwtHelper, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHelper = jwtHelper;
            _IEmailSender = emailSender;
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

                if (signResult.Succeeded)
                {
                    var user = await _userManager.Users.FirstOrDefaultAsync(i => i.NormalizedUserName.Equals(model.UserName.ToUpper()));

                    var token = _jwtHelper.GetToken(user.Id);
                    var key = _jwtHelper.GetTokenValue(token);
                    result.Succeeded = true;
                    UserData data = new UserData(key, User.GetUserId());
                    result.Data = data;
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
                var user = new ApplicationUser { UserName = model.UserName, FirstAndLastName = model.FirstAndLastName, Email = model.Email };
                var isUsernametaken = await _userManager.FindByNameAsync(model.UserName);
                if (isUsernametaken != null)
                {
                    result.Message = "Username is already taken, please take a new username.";
                }
                else
                {
                    var Registerresult = await _userManager.CreateAsync(user, model.Password);
                    if (Registerresult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Message = string.Join(',', Registerresult.Errors.Select(i => i.Description));
                    }
                }
            }
            else
            {
                result.Message = "Cannot be empty";
            }

            return Json(result);
        }

        [HttpGet("[action]"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUser(string Id)
        {
            ResultModel result = new ResultModel();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(Id);

                if (user != null)
                {
                    result.Succeeded = true;
                    result.Message = "User is found.";
                    var data = new GetUser(user.Id, user.UserName, user.FirstAndLastName, user.Email);
                    result.Data = data;
                }
                else
                    result.Message = "User is not found.";
            }
            return Json(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordViewModel model)
        {
            ResultModel result = new ResultModel();
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    result.Message = "This email is not found.";
                }
                else
                {
                    var key = Helper.ShortIdentifier();
                    user.ForgotPasswordKey = key;
                    await _userManager.UpdateAsync(user);
                    var link = GetResetLink(user.ForgotPasswordKey);
                    var emailMessage = $"<a href='{link}'>{link}</a>";
                    await _IEmailSender.SendEmailAsync(model.Email, "Forgot Password", emailMessage);
                    result.Succeeded = true;
                    result.Message = "Reset password link sended your email.";
                }
            }
            return Json(result);
        }
        private string GetResetLink(string hash) => $"{this.Request.Scheme}://{this.Request.Host}/resetpass/{hash}";
        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordViewModel model)
        {
            ResultModel result = new ResultModel();
            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(i => i.ForgotPasswordKey.Equals(model.Key));
                var result1 = await _userManager.RemovePasswordAsync(user);
                var result2 = await _userManager.AddPasswordAsync(user, model.Password);
                if (result1.Succeeded && result2.Succeeded)
                {
                    result.Succeeded = true;
                    result.Message = "Password changed succesfully.";
                    user.ForgotPasswordKey = string.Empty;
                    await _userManager.UpdateAsync(user);
                }

            }
            return Json(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> HashControl([FromBody]KeyControlViewModel model)
        {
            ResultModel result = new ResultModel();
            if (ModelState.IsValid)
            {
                var find = await _userManager.Users.AnyAsync(i => i.ForgotPasswordKey.Equals(model.Key));
                result.Succeeded = find;
            }
            return Json(result);
        }
    }
}