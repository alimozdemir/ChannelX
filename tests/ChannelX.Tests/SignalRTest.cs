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

namespace ChannelX.Tests
{
    public class HubConnectionTest
    {
        [Fact]
        public async Task ConnectionTest()
        {
            var mockObject = new Mock<IConnection>();
            mockObject.SetupGet(p => p.Features).Returns(new FeatureCollection());
            mockObject.Setup(p => p.StartAsync()).Returns(Task.CompletedTask).Verifiable();
            HubConnection conn = new HubConnection(mockObject.Object, Mock.Of<IHubProtocol>(),null);
            await conn.StartAsync();
            
            mockObject.Verify(c => c.StartAsync(), Times.Once());
        }
    }
}

