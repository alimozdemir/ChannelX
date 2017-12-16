using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Email;
using ChannelX.Models;
using ChannelX.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace ChannelX.Tests
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            //Configuration = configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("emailsettings.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(o => o.UseInMemoryDatabase("InMemoryDb"));

            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            services.AddIdentity<Data.ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<Data.DatabaseContext>()
                    .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(i =>
            {
                i.LoginPath = "/login";

            });

            var tokenConfiguration = Configuration.GetSection("Tokens");

            services.AddAuthentication()
                    .AddJwtBearer(options =>
                    {
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
            services.Configure<Models.Configuration.EmailSettings>(Configuration.GetSection("EmailSettings"));
            // Registering email service
            services.AddTransient<IEmailSender, AuthMessageSender>();
            // Registering true email service

            services.AddScoped<SendBulkEmail>();

            //services.AddQuartz();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {


            AppServices = app.ApplicationServices;
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();

            app.UseDeveloperExceptionPage();
            app.UseWebpackDevMiddleware(new Microsoft.AspNetCore.SpaServices.Webpack.WebpackDevMiddlewareOptions
            {
                HotModuleReplacement = true
            });

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

            var context = app.ApplicationServices.GetService<DatabaseContext>();
            InitializeDatabase(context);
        }

        public void InitializeDatabase(DatabaseContext context)
        {
            var ch1 = new Channel()
            {
                CreatedAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(5),
                Title = "FirstOne",
                OwnerId = MockData.FirstUserId
            };
            var ch2 = new Channel()
            {
                CreatedAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(5),
                Title = "SecondOne",
                OwnerId = MockData.SecondUserId
            };
            var ch3 = new Channel()
            {
                CreatedAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(5),
                Title = "ChannelWithPassword",
                OwnerId = MockData.FirstUserId,
                Password = "1"
            };

            var ch4 = new Channel()
            {
                CreatedAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(5),
                Title = "ChannelWithPassword",
                OwnerId = MockData.SecondUserId,
                Password = "1"
            };

            var ch5 = new Channel()
            {
                CreatedAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(5),
                Title = "ChannelHash",
                OwnerId = MockData.SecondUserId,
                Hash = Helper.ShortIdentifier()
            };


            // Public First User
            context.Channels.Add(ch1);
            // Public Second User
            context.Channels.Add(ch2);

            // Public First User With Password
            context.Channels.Add(ch3);

            // Public Second User With Password
            context.Channels.Add(ch4);

            // Public Second User With Hash Value
            context.Channels.Add(ch5);

            context.SaveChanges();

            MockData.CPublicFirstUser = ch1.Id;
            MockData.CPublicSecondUser = ch2.Id;
            MockData.CPublicFirstUserWithPassword = ch3.Id;
            MockData.CPublicSecondUserWithPassword = ch4.Id;
            MockData.CPublicSecondUserHash = ch5.Hash;

        }

        public void SetApplicationUsers(IApplicationBuilder app)
        {
            var _userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
            {
                var user = new ApplicationUser { UserName = "deneme", FirstAndLastName = "alim", Email = "ozdemirali@itu.edu.tr" };
                var id_result = _userManager.CreateAsync(user, "Deneme123!");
                UserForgotPasswordKey = Helper.ShortIdentifier();
                MockData.FirstUserId = user.Id;
                user.ForgotPasswordKey = UserForgotPasswordKey;

                var _jwtHelper = app.ApplicationServices.GetService<Token.JwtSecurityHelper>();
                var token = _jwtHelper.GetToken(user.Id);
                var key = _jwtHelper.GetTokenValue(token);
                MockData.AuthKey = key;

                var secondUser = new ApplicationUser { UserName = "Deneme1", FirstAndLastName = "alim2", Email = "alm.ozdmr@live.com" };
                _userManager.CreateAsync(secondUser, "Deneme123!");
                MockData.SecondUserId = secondUser.Id;
            }
        }

        // General purpose auth key for 'deneme' user

        public static string UserForgotPasswordKey { get; set; }
        public static IServiceProvider AppServices { get; set; }
    }

    public class MockData
    {
        public static string AuthKey { get; set; }
        public static string FirstUserId { get; set; }
        public static string SecondUserId { get; set; }

        public static int CPublicFirstUser { get; set; }
        public static int CPublicSecondUser { get; set; }
        public static int CPublicFirstUserWithPassword { get; set; }
        public static int CPublicSecondUserWithPassword { get; set; }
        public static string CPublicSecondUserHash { get; set; }
    }
}