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
            var key = Startup.AuthKey;
            var user_id = Startup.UserId;
            var app_services = Startup.AppServices;
            var service = (DatabaseContext) app_services.GetService(typeof(DatabaseContext));
            var data = service.Channels;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            var response = await _client.GetAsync("/api/Channel/Public");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ListModel>(content);
            Assert.Equal(result.Title,"FirstOne");
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task Engaged()
        {
            var key = Startup.AuthKey;
            var user_id = Startup.UserId;
            var app_services = Startup.AppServices;
            var service = (DatabaseContext) app_services.GetService(typeof(DatabaseContext));
            var data = service.Channels;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            var response = await _client.GetAsync("/api/Channel/Engaged");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ListModel>>(content);
            Assert.Equal(result[0].Title,"FirstOne"); // fails for now because user is needed to engage
            _client.DefaultRequestHeaders.Clear();
        }
        [Fact]
        public async Task Get()
        {
            var key = Startup.AuthKey;
            var user_id = Startup.UserId;
            var app_services = Startup.AppServices;
            var service = (DatabaseContext) app_services.GetService(typeof(DatabaseContext));
            var data = service.Channels;
            int id = 0;
            foreach(var channel in data)
            {
                id = channel.Id;
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            IdFormModel model = new IdFormModel();
            model.Id = id;

            var response = await _client.PostAsync("/api/Channel/Get",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);
            Assert.Equal(result.Data.Title,"FirstOne");
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task GetHash()
        {
            
            var key = Startup.AuthKey;
            var user_id = Startup.UserId;
            var app_services = Startup.AppServices;
            var service = (DatabaseContext) app_services.GetService(typeof(DatabaseContext));
            var data = service.Channels;
            string hash = String.Empty;
            foreach(var channel in data)
            {
                hash = channel.Hash;
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            IdStringFormModel model = new IdStringFormModel();
            model.Id = hash;

            var response = await _client.PostAsync("/api/Channel/GetHash",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);
            Assert.True(result.Succeeded);
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task GetWithPassword()
        {
            
            var key = Startup.AuthKey;
            var user_id = Startup.UserId;
            var app_services = Startup.AppServices;
            var service = (DatabaseContext) app_services.GetService(typeof(DatabaseContext));
            var data = service.Channels;
            int id = 0;
            string pw = String.Empty;
            foreach(var channel in data)
            {
                id = channel.Id;
                pw = channel.Password;
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            PasswordFormModel model = new PasswordFormModel();
            model.Id = id;
            model.Password = pw;

            var response = await _client.PostAsync("/api/Channel/GetWithPassword",
                new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResultModel<ListModel>>(content);
            Assert.True(result.Succeeded);
            _client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task HistoryPage()
        {
            var key = Startup.AuthKey;
            var user_id = Startup.UserId;
            var app_services = Startup.AppServices;
            var service = (DatabaseContext) app_services.GetService(typeof(DatabaseContext));
            var data = service.Channels;
            foreach(var channel in data)
            {
                System.Diagnostics.Debug.Write(channel.Id);
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            HistoryPaginationModel model = new HistoryPaginationModel();
            model.Count  =1;
            model.CurrentPage =1;
            model.Total = 1;
            var response = await _client.GetAsync($"/api/Channel/HistoryPage/?Count={model.Count}&CurrentPage={model.CurrentPage}&Total={model.Total}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            //var result = JsonConvert.DeserializeObject<HistoryModel>(content);
            Assert.Equal(content,"[]");
            _client.DefaultRequestHeaders.Clear();
        }
    }
}
