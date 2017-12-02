using System;
using System.Net.Http;
using System.Threading.Tasks;
using ChannelX.Models;
using ChannelX.Models.Account;
using ChannelX.Tests.Fixtures;
using Newtonsoft.Json;
using Xunit;

namespace ChannelX.Tests
{
    public class UnitTest1 : IClassFixture<TestFixture<Startup>>
    {
        private readonly HttpClient _client;

        public UnitTest1(TestFixture<Startup> fixture)
        {
            _client = fixture.Client;
        }


        [Fact]
        public async Task Login()
        {
            AccountViewModel model = new AccountViewModel();
            model.UserName = "deneme";
            model.Password = "Deneme123!";


            var response = await _client.PostAsync("/api/Account/Login", 
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
            
            //Console.WriteLine(result.Message);
            //Assert.NotNull(response);
        }
    }
}
