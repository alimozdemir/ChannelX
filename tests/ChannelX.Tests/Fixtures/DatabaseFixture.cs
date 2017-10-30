using System;
using Xunit;
using Moq;
using ChannelX.Data;
using Microsoft.EntityFrameworkCore;

namespace ChannelX.Tests.Fixtures 
{
    public class DatabaseFixture : IDisposable
    {
        readonly DatabaseContext context;
        public DatabaseFixture()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseInMemoryDatabase("InMemory");

            context = new DatabaseContext(builder.Options);

            
        }

        public void InitChannels()
        {
            
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}