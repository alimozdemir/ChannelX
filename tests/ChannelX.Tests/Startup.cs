using System;
using System.Collections;
using System.Threading.Tasks;
using ChannelX.Data;
using ChannelX.Email;
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
            
            services.ConfigureApplicationCookie(i => {
                i.LoginPath = "/login";
                 
            });

            var tokenConfiguration = Configuration.GetSection("Tokens");
            
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
            services.Configure<Models.Configuration.EmailSettings>(Configuration.GetSection("EmailSettings"));
            // Registering email service
            services.AddTransient<IEmailSender, AuthMessageSender>();
            // Registering true email service

            services.AddScoped<SendBulkEmail>();

            //services.AddQuartz();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            /*using(var context = app.ApplicationServices.GetService<DatabaseContext>())
            {
                InitializeDatabase(context);
            }*/
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

                var _jwtHelper = app.ApplicationServices.GetService<Token.JwtSecurityHelper>();
                var token = _jwtHelper.GetToken(user.Id);
                var key = _jwtHelper.GetTokenValue(token);
                AuthKey = key;
            }
        }

        // General purpose auth key for 'deneme' user
        public static string AuthKey {get; set;}

    }
}