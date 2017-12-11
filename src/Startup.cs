using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
// using Microsoft.Extensions.Caching.Redis;
using ChannelX.Redis;
using ChannelX.Email;
using Microsoft.AspNetCore.HttpOverrides;

namespace ChannelX
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("emailsettings.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<Data.DatabaseContext>(o => {
                o.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
             	
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

                            ValidIssuer = tokenConfiguration.GetValue<string>("Issuer"),
                            ValidAudience = tokenConfiguration.GetValue<string>("Audience"),
                            IssuerSigningKey = Token.JwtSecurityHelper.Key(tokenConfiguration.GetValue<string>("Key"))
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

            var redisConfiguration = Configuration.GetSection("Redis");

            services.Configure<Models.Configuration.Redis>(redisConfiguration);
            
            services.AddSingleton<Token.JwtSecurityHelper>();

            services.AddSignalR();

            services.AddSingleton<Models.Trackers.UserTracker>();

            // EMAIL PART
            // load email settings if it is avaliable
            services.Configure<Models.Configuration.EmailSettings>(Configuration.GetSection("EmailSettings"));
            // Registering email service
            services.AddTransient<IEmailSender, AuthMessageSender>();
            // Registering true email service

            services.AddScoped<SendBulkEmail>();
            services.AddQuartz();
 
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

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
        }
    }
}
