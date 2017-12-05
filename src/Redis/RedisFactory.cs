
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

// https://garywoodfine.com/redis-inmemory-cache-asp-net-mvc-core/

namespace ChannelX.Redis
{ 
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer Connection();
        bool IsConnected {get;}
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        /// <summary>
        ///     The _connection.
        /// </summary>
        private readonly ConnectionMultiplexer _connection;
        public bool IsConnected{get;}

        public RedisConnectionFactory()
        {
            try{
                this._connection = ConnectionMultiplexer.Connect("localhost:6379"); // change connection String
                IsConnected = true;
            }
            catch{
                // will handle in the future
                IsConnected = false;
            }
            
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection;
        }
    }


}