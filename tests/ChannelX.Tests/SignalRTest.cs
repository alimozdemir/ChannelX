using System;
using Xunit;
using ChannelX.Redis;
using Moq;
using StackExchange.Redis;
using ChannelX.Models.Trackers;
using ChannelX.Models.Chat;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Sockets.Client;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
using ChannelX.Tests.Fixtures;

namespace ChannelX.Tests
{
    public class HubConnectionTest : IClassFixture<TestFixture<Startup>>
    {
        TestFixture<Startup> _fixture;

        public HubConnectionTest(TestFixture<Startup> fixture)
        {
            _fixture = fixture;
            
        }

        /*[Fact]
        public async Task ConnectionTest()
        {
            var mockObject = new Mock<IConnection>();
            mockObject.SetupGet(p => p.Features).Returns(new FeatureCollection());
            mockObject.Setup(p => p.StartAsync()).Returns(Task.CompletedTask).Verifiable();
            HubConnection conn = new HubConnection(mockObject.Object, Mock.Of<IHubProtocol>(),null);
            await conn.StartAsync();
            
            mockObject.Verify(c => c.StartAsync(), Times.Once());
        }*/

        [Fact]
        public async Task ConnectionTest1()
        {
            var response = await _fixture.Client.GetAsync("http://localhost:5000/api/chat?token="+Startup.AuthKey);
            Console.WriteLine(response);

            
            
            
            
            
            Console.WriteLine("SignalR HubConnectionBuilder");
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/api/chat?token="+Startup.AuthKey)
                .WithConsoleLogger()
                .Build();
                
            await connection.StartAsync();
        }


    }
}

