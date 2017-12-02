using System;
using System.Collections;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace ChannelX.Tests
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(o => o.UseInMemoryDatabase("InMemoryDb"));


            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            services.AddIdentity<Data.ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<Data.DatabaseContext>()
                    .AddDefaultTokenProviders();
            
            services.ConfigureApplicationCookie(i => {
                i.LoginPath = "/login";
                 
            });

            var tokenConfiguration = Configuration.GetSection("Tokens");

            Console.WriteLine(tokenConfiguration.GetValue<string>("Key"));

            services.AddAuthentication()
                    .AddJwtBearer(options => {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = "ChannelX",
                            ValidAudience = "ChannelX",
                            IssuerSigningKey = Token.JwtSecurityHelper.Key("5fdc4141-0815-4fa9-8c69-f25200e1831a")
                        };
                        
                        options.SaveToken = true;
                        options.Events = new JwtBearerEvents();
                        options.Events.OnMessageReceived = context =>
                        {
                            StringValues token;
                            if (context.Request.Path.Value.StartsWith("/api/chat") && context.Request.Query.TryGetValue("token", out token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        };
                    });

            services.AddMvc();

            services.Configure<Models.Configuration.Tokens>(tokenConfiguration);

            services.AddSingleton<Token.JwtSecurityHelper>();

            services.AddSignalR();

            services.AddSingleton<Models.Trackers.UserTracker>();
        }

        public void Configure(IApplicationBuilder app)
        {
            /*using(var context = app.ApplicationServices.GetService<DatabaseContext>())
            {
                InitializeDatabase(context);
            }*/

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSignalR(routes =>
            {
                routes.MapHub<Hubs.Chat>("api/chat");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });

            SetApplicationUsers(app);
        }

        public void InitializeDatabase(DatabaseContext context)
        {
            context.Channels.Add(new Channel() {
                CreatedAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(5),
                Title = "FirstOne"
            });
        }

        public void SetApplicationUsers(IApplicationBuilder app) 
        {
            var _userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
            {
                Console.WriteLine(_userManager == null ? "is null" : "not null");
                var user = new ApplicationUser { UserName = "deneme",FirstAndLastName="alim",Email = "ozdemirali@itu.edu.tr" };
                _userManager.CreateAsync(user, "Deneme123!");
            }
        }

        /*public IEnumerable<Channel> GetChannelSession()
        {

        }*/
    }
}