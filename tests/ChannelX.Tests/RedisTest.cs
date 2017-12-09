using System;
using Xunit;
using ChannelX.Redis;
using Moq;
using StackExchange.Redis;
using ChannelX.Models.Trackers;
using ChannelX.Models.Chat;

namespace ChannelX.Tests
{
    public class RedisTest
    {
        Mock<RedisConnection> mockObject;
        public RedisTest()
        {

            mockObject = new Mock<RedisConnection>();
            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            var mockDatabase = new Mock<IDatabase>();
            mockObject.Object._connection = mockMultiplexer.Object;

            mockMultiplexer.Setup(_ => _.IsConnected).Returns(true);
            mockMultiplexer
                .Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(mockDatabase.Object);
        }


        [Fact]
        public void ConnectTest()
        {

            // var mockObject = new Mock<RedisConnection>();
            // mockObject.Setup(i => i._connection.IsConnected).Returns(true);
            Assert.True(mockObject.Object._connection.IsConnected);
        }

        [Fact]
        public void HashSetTest()
        {
            // var mockObject = new Mock<RedisConnection>();
            // var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            // var mockDatabase = new Mock<IDatabase>();
            // mockObject.Object._connection = mockMultiplexer.Object;

            // mockMultiplexer.Setup(_ => _.IsConnected).Returns(true);
            // mockMultiplexer
            //     .Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            //     .Returns(mockDatabase.Object);

            string key = "alim";
            HashEntry[] value = new HashEntry[1];
            value[0] = new HashEntry("Hey", "Hey");
            
            var obj = new RedisConnection(mockObject.Object);
            Assert.True(obj.HashSet(key,value));
        }

        [Fact]
        public void ListPushTest()
        {

            string key = "alim";
            string value = "hey";
            var obj = new RedisConnection(mockObject.Object);
            Assert.True(obj.ListRightPush(key,value));
        }

        [Fact]
        public void ListRangeTest()
        {
            int start = 0;
            int stop = -1;
            string key = "alim";
            var obj = new RedisConnection(mockObject.Object);
            Assert.Equal(obj.ListRange(key,start,stop),new RedisValue[0]);
        }

        [Fact]
        public void HashGetTest()
        {
            string key = "alim";
            string field = "hey";
            var obj = new RedisConnection(mockObject.Object);
            Assert.Equal(obj.HashGet(key,field),new RedisValue());
        }

        [Fact]
        public void HashGetAllTest()
        {
            string key = "alim";
            string field = "hey";
            var obj = new RedisConnection(mockObject.Object);
            Assert.Equal(obj.HashGetAll(key),new HashEntry[0]);
        }
        [Fact]
        public void InsertMessageTest()
        {
            UserDetail user = new UserDetail(new Guid().ToString(),"alim","20","1",1);
            TextModel message = new TextModel();
            message.Content = "message";
            message.SentTime = DateTime.Now;
            message.User = user;
            var obj = new RedisConnection(mockObject.Object);
            Assert.True(obj.InsertMessage(user,message));

        }
        [Fact]
        public void UpdateLastSeenTest()
        {
            UserDetail user = new UserDetail(new Guid().ToString(),"alim","20","1",1);
            var obj = new RedisConnection(mockObject.Object);
            Assert.True(obj.UpdateLastSeen(user));
            
        }
    }
}
