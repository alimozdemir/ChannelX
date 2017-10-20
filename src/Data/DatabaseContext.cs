using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ChannelX.Data
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<Channel>(entity =>
            {

                entity.HasOne(i => i.Owner)
                    .WithMany(i => i.Channels)
                    .HasForeignKey(i => i.OwnerId);

            });

            builder.Entity<ChannelUser>(entity =>
            {

                entity.HasKey(i => new { i.ChannelId, i.UserId });

                entity.HasOne(i => i.User)
                    .WithMany(i => i.EngagedChannels)
                    .HasForeignKey(i => i.UserId);

                entity.HasOne(i => i.Channel)
                    .WithMany(i => i.Users)
                    .HasForeignKey(i => i.ChannelId);
            });

        }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelUser> ChannelUsers { get; set; }
    }

}