using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ChannelX.Models;
using ChannelX.Models.Account;
using ChannelX.Models.Channel;
using ChannelX.Tests.Fixtures;
using Newtonsoft.Json;
using Xunit;

namespace ChannelX.Tests
{
    public class ChannelControllerTest : IClassFixture<TestFixture<Startup>>
    {
        private readonly HttpClient _client;

        public ChannelControllerTest(TestFixture<Startup> fixture)
        {
            _client = fixture.Client;
        }


        [Fact]
        public async Task Create()
        {
            var key = Startup.AuthKey;
            var user_id = Startup.UserId;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            CreateModel model = new CreateModel();
            model.Password = "";
            model.EndAtHours = 2;
            model.IsPrivate = false;
            model.Title = "Hello";

            var response = await _client.PostAsync("/api/Channel/Create",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task Public()
        {
            // cannot be tested
        }

        [Fact]
        public async Task Engaged()
        {
            // cannot be tested
        }
        [Fact]
        public async Task Get()
        {
            IdFormModel model = new IdFormModel();
            model.Id = 0;

            var response = await _client.PostAsync("/api/Channel/Get",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task GetHash()
        {
            IdStringFormModel model = new IdStringFormModel();
            model.Id = ""; // need hash here

            var response = await _client.PostAsync("/api/Channel/GetHash",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task GetWithPassword()
        {
            PasswordFormModel model = new PasswordFormModel();
            model.Id = 0;
            model.Password = "Deneme123!";
            var response = await _client.PostAsync("/api/Channel/GetWithPassword",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultModel>(content);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task HistoryPage()
        {
            // cannot be tested
        }
    }
}
