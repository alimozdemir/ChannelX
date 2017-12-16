using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Models;
using ChannelX.Models.Account;
using ChannelX.Models.Channel;
using ChannelX.Tests.Fixtures;
using Newtonsoft.Json;
using Xunit;
using System.Linq;

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
        public async Task CreateWithValidData()
        {
            var key = MockData.AuthKey;
            var user_id = MockData.FirstUserId;
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
        public async Task PublicWithSecondUser()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);

            var response = await _client.GetAsync("/api/Channel/Public");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ListModel>>(content);
            Assert.True(result.Count > 0);
            //Assert.Contains("SecondOne", result.Select(i => i.Title));
            //Assert.Equal("SecondOne", result[0].Title);
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task EngagedWithFirstUser()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);

            var response = await _client.GetAsync("/api/Channel/Engaged");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ListModel>>(content);
            Assert.True(result.Count > 0); // fails for now because user is needed to engage
            _client.DefaultRequestHeaders.Clear();
        }
        [Fact]
        public async Task GetWithNoPasswordPublicUserOne()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            IdFormModel model = new IdFormModel();
            model.Id = MockData.CPublicSecondUser; // second user's channel it should proceed

            var response = await _client.PostAsync("/api/Channel/Get",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);

            Assert.True(result.Succeeded);
            Assert.Equal(MockData.CPublicSecondUser, result.Data.Id);
            _client.DefaultRequestHeaders.Clear();
        }
        [Fact]
        public async Task GetWithPasswordPublicUserOne()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            IdFormModel model = new IdFormModel();
            model.Id = MockData.CPublicSecondUserWithPassword; // second user's channel it should proceed

            var response = await _client.PostAsync("/api/Channel/Get",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<object>>(content);

            Assert.True(result.Prompt);
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task GetWithInvalidChannelNumber()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            IdFormModel model = new IdFormModel();
            model.Id = 99; // second user's channel it should proceed

            var response = await _client.PostAsync("/api/Channel/Get",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);
            Assert.False(result.Succeeded);
            //Assert.Equal(MockData.CPublicSecondUser, result.Data.Id);
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task GetHashWithValidData()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);

            IdStringFormModel model = new IdStringFormModel();
            model.Id = MockData.CPublicSecondUserHash;
            Console.WriteLine("Hash:" + model.Id);
            System.Diagnostics.Debug.WriteLine("Hash:" + model.Id);
            var response = await _client.PostAsync("/api/Channel/GetHash",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);
            Console.WriteLine("Hash1:" + content);
            System.Diagnostics.Debug.WriteLine("Hash1:" + content);
            Assert.True(result.Succeeded);
            
            _client.DefaultRequestHeaders.Clear();
        }
        [Fact]
        public async Task GetHashWithInvalidData()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            IdStringFormModel model = new IdStringFormModel();
            model.Id = "invalid";

            var response = await _client.PostAsync("/api/Channel/GetHash",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);
            Assert.False(result.Succeeded);
            _client.DefaultRequestHeaders.Clear();
        }
        
        [Fact]
        public async Task GetWithPassword()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            PasswordFormModel model = new PasswordFormModel();
            model.Id = MockData.CPublicSecondUserWithPasswordTwo;
            model.Password = "1";

            var response = await _client.PostAsync("/api/Channel/GetWithPassword",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);
            Assert.True(result.Succeeded);
            _client.DefaultRequestHeaders.Clear();
        } 

        [Fact]
        public async Task HistoryPageWithValidData()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            HistoryPaginationModel model = new HistoryPaginationModel();
            model.Count = 10;
            model.CurrentPage = 1;
            model.Total = 2;
            var response = await _client.GetAsync($"/api/Channel/HistoryPage/?Count={model.Count}&CurrentPage={model.CurrentPage}&Total={model.Total}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            //var result = JsonConvert.DeserializeObject<HistoryModel>(content);
            Assert.NotEqual("[]", content);
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task HistoryPageWithInvalidData()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            HistoryPaginationModel model = new HistoryPaginationModel();
            model.Count = -1;
            model.CurrentPage = -1;
            model.Total = 10;
            var response = await _client.GetAsync($"/api/Channel/HistoryPage/?Count={model.Count}&CurrentPage={model.CurrentPage}&Total={model.Total}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            //var result = JsonConvert.DeserializeObject<HistoryModel>(content);
            Assert.Equal(content, "null");
            _client.DefaultRequestHeaders.Clear();
        }
        
        [Fact]
        public async Task HistoryPageTotalCount()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MockData.AuthKey);
            HistoryPaginationModel model = new HistoryPaginationModel();
            model.Count = 1;
            model.CurrentPage = 1;
            model.Total = 1;
            var response = await _client.GetAsync($"/api/Channel/HistoryPageTotal");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<int>(content);
            Assert.True(result > 0);
            _client.DefaultRequestHeaders.Clear();
        }
    }
}
