using System;

namespace Microsoft.AspNetCore.SignalR
{
    public static class HubExtensions
    {
        public static IClientProxy AllExcept(this IHubClients hub, string id)
        {
            return hub.AllExcept(new string[1] { id });
        }
    }
}