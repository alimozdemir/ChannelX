using System;
using Microsoft.AspNetCore.Sockets;
using Microsoft.AspNetCore.Sockets.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
using Microsoft.AspNetCore.Sockets.Features;
using Moq;

namespace ChannelX.Tests.Fixtures
{
    public class SocketConnectionFixture : IDisposable
    {
        public IConnection Connection { get; }
        public HubConnection HubConnection { get; }

        public SocketConnectionFixture() : this("http://localhost:5000/api/chat")
        {

        }
        protected SocketConnectionFixture(string url)
        {
            var uri = new Uri(url);
            Connection = new HttpConnection(uri);
            
            HubConnection = new HubConnection(Connection, Mock.Of<IHubProtocol>(), null);
            //HubConnection = new HubConnection(Connection, TransportType.LongPolling, null);
        }

        public void Dispose()
        {
            Connection.DisposeAsync().Wait();
        }
    }
}