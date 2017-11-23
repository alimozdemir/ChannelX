
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
        private readonly ConnectionMultiplexer _connection;
        

        public RedisConnectionFactory()
        {
            try{
                this._connection = ConnectionMultiplexer.Connect("localhost:6379"); // change connection String   
            }
            catch{
                // will handle in the future
            }
            
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection;
        }
    }


}