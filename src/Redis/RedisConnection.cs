using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using ChannelX.Models.Trackers;
using ChannelX.Models.Chat;

namespace ChannelX.Redis
{ 

    public class RedisConnection
    {
        public IConnectionMultiplexer _connection;
        public RedisConnection()
        {
            try{
                this._connection = ConnectionMultiplexer.Connect("localhost:6379"); // change connection String
            }
            catch{
            }
            
        }
        public RedisConnection(RedisConnection other)
        {
            this._connection = other._connection;
        }

        private IDatabase GetDatabase()
        { 
            return this._connection.GetDatabase();
        }
    
        public virtual bool HashSet(string key, HashEntry[] arr )
        {
            if(this._connection.IsConnected)
            {
                var db = this.GetDatabase();
                db.HashSet(key,arr);
                return true; // check this at unit tests
            }
            else
            {
                return false;
            } 
        }
        public RedisValue HashGet(string key, string hashField)
        {
            RedisValue ret = new RedisValue();
            if(this._connection.IsConnected)
            {
                var db = this.GetDatabase();
                ret = db.HashGet(key,hashField);
            }
            return ret;
        }
        public HashEntry[] HashGetAll(string key)
        {
            HashEntry[] ret = new HashEntry[0];
            if(this._connection.IsConnected)
            {
                var db = this.GetDatabase();
                ret = db.HashGetAll(key);
            }
            return ret;
        }

        public RedisValue[] ListRange(string key, int start, int stop)
        {
            RedisValue[] ret = new RedisValue[0];
            if(this._connection.IsConnected)
            {
                var db = this.GetDatabase();
                ret = db.ListRange(key,start,stop);
            }
            return ret;
        }

        public bool ListRightPush(string key, string val)
        {
            if(this._connection.IsConnected)
            {
                var db = this.GetDatabase();
                db.ListRightPush(key,val);
                return true; // check this at unit tests
            }
            else
            {
                return false;
            } 
        }
        public bool InsertMessage(UserDetail user, TextModel message)
        {
            if(this._connection.IsConnected)
            {
                var db = this.GetDatabase();
                db.ListRightPush(user.GroupId.ToString(), message.ToString());
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateLastSeen(UserDetail user)
        {
            if(this._connection.IsConnected)
            {
                var db = this.GetDatabase();
                HashEntry entry = new HashEntry(user.UserId.ToString(), DateTime.Now.ToString());
                HashEntry[] arr = new HashEntry[1];
                arr[0] = entry;
                db.HashSet("LastSeen" + user.GroupId.ToString(), arr);
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
