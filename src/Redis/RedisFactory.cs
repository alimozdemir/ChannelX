
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

// https://garywoodfine.com/redis-inmemory-cache-asp-net-mvc-core/

namespace ChannelX.Redis
{ 
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer Connection();
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        /// <summary>
        ///     The _connection.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connection;
        

        public RedisConnectionFactory()
        {
            this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("localhost:6379")); // change connection String
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection.Value;
        }
    }


}