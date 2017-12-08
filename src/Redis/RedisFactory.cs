
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

// https://garywoodfine.com/redis-inmemory-cache-asp-net-mvc-core/

namespace ChannelX.Redis
{ 
    public interface IRedisConnectionFactory
    {
        RedisConnection Connection();
      
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        RedisConnection _connection_wrapper;
        public RedisConnectionFactory()
        {
            _connection_wrapper = new RedisConnection();
        }

        public RedisConnection Connection()
        {
            return this._connection_wrapper;
        }
    }


}