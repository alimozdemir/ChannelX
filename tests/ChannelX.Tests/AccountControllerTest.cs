using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ChannelX.Models;
using ChannelX.Models.Account;
using ChannelX.Tests.Fixtures;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace ChannelX.Tests
{
    
    public class AccountControllerTest : IClassFixture<TestFixture<Startup>>
    {
        private readonly HttpClient _client;

        public AccountControllerTest(TestFixture<Startup> fixture)
        {
            _client = fixture.Client;
        }


        [Fact]
        public async Task Login()
        {
            AccountViewModel model = new AccountViewModel();
            model.UserName = "deneme";
            model.RememberMe = false;
            model.Password = "Deneme123!";
            // initialize database

            var response = await _client.PostAsync("/api/Account/Login", 
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
        }
        
        [Fact]
        public async Task RegisterPasswordPass()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.UserName = "alim";
            model.Email = "alim@itu.edu.tr";
            model.ConfirmPassword = "P11w1!";
            model.Password = "P11w1!";
            model.FirstAndLastName = "alim ozdemir";
        
            var response = await _client.PostAsync("/api/Account/Register",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);

        }
        [Fact]
        public async Task RegisterPasswordLengthCriteria()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.UserName = "alim";
            model.Email = "alim@itu.edu.tr";
            model.ConfirmPassword = "pw";
            model.Password = "pw";
            model.FirstAndLastName = "alim ozdemir";
        
            var response = await _client.PostAsync("/api/Account/Register",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.False(result.Succeeded);

        }
        [Fact]
        public async Task RegisterPasswordContainsCriteria()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.UserName = "alim";
            model.Email = "alim@itu.edu.tr";
            model.ConfirmPassword = "pwasdfasdf";
            model.Password = "pwasdfasdf";
            model.FirstAndLastName = "alim ozdemir";
        
            var response = await _client.PostAsync("/api/Account/Register",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.False(result.Succeeded);

        }
        [Fact]
        public async Task RegisterPasswordNoMatch()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.UserName = "alim";
            model.Email = "alim@itu.edu.tr";
            model.ConfirmPassword = "pw1";
            model.Password = "pw";
            model.FirstAndLastName = "alim ozdemir";
        
            var response = await _client.PostAsync("/api/Account/Register",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.False(result.Succeeded);

        }
        [Fact]
        public async Task GetUser()
        {
            // var user_id  = Startup.UserId;
            // var key = Startup.AuthKey; // bunu request e ekle
            // _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            // var response = await _client.GetAsync($"/api/Account/GetUser/?id={user_id}");
            // response.EnsureSuccessStatusCode();
            // var content = await response.Content.ReadAsStringAsync();
            // var actual_result = JsonConvert.DeserializeObject<TResultModel<GetUser>>(content);
            
            // // GetUser data = new GetUser(user_id.ToString(),"deneme","alim","ozdemirali@itu.edu.tr");
            // // TResultModel<GetUser> expected = new TResultModel<GetUser>(){
            // //     Succeeded = true,
            // //     Message = "User is found.",
            // //     Data = data,
            // //     Prompt = false};
            // Assert.True(actual_result.Succeeded);
            // // Assert.Equal("deneme",actual_result.Data.UserName);
            // _client.DefaultRequestHeaders.Clear();
        }
        [Fact]
        public async Task ForgotPasswordNoEmail()
        {
            ForgotPasswordViewModel model = new ForgotPasswordViewModel();
            model.Email = "alim@itu.edu.tr";
        
            var response = await _client.PostAsync("/api/Account/ForgotPassword",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.False(result.Succeeded);
        }
        [Fact]
        public async Task ForgotPasswordPass()
        {
            ForgotPasswordViewModel model = new ForgotPasswordViewModel();
            model.Email = "ozdemirali@itu.edu.tr";
        
            var response = await _client.PostAsync("/api/Account/ForgotPassword",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
        }
        [Fact]
        public async Task ResetPassword()
        {
            /*
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Password = "aAaaaaa1!";
            model.ConfirmPassword = "aAaaaaa1!";
            model.Key = Startup.UserForgotPasswordKey;
            
            var response = await _client.PostAsync("/api/Account/ResetPassword",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
             */
        }
        [Fact]
        public async Task ResetPasswordNoMatch()
        {
            /* ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Password = "aaa";
            model.ConfirmPassword = "aa";
            model.Key = String.Empty;
            
            var response = await _client.PostAsync("/api/Account/ResetPassword",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.False(result.Succeeded);*/
        }
    }
}
