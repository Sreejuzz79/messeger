using System;
using MessangerWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebsiteApplication.Services;

namespace WebsiteApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure forwarded headers for reverse proxy (Render)
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddMemoryCache();

            // Configure Authentication FIRST
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "WebsiteApplication.Auth";
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

            // Add SignalR with configuration
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
                options.StreamBufferCapacity = 1024 * 1024; // 1MB
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });

            // Add logging
            builder.Services.AddLogging();

            // Add your custom services
            builder.Services.AddScoped<PostgreSqlConnectionService>();
            builder.Services.AddScoped<IVideoCallHistoryService, VideoCallHistoryService>();
            builder.Services.AddScoped<IVideoCallParticipantService, VideoCallParticipantService>();

            // Add these services
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<NotificationService>();

            // Add HTTP context accessor
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Initialize database tables on startup
            using (var scope = app.Services.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<PostgreSqlConnectionService>();
                try
                {
                    var connection = dbService.GetConnectionAsync().Result;
                    var createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS students (
                            id SERIAL PRIMARY KEY,
                            firstname VARCHAR(100),
                            lastname VARCHAR(100),
                            gender VARCHAR(50),
                            dateofbirth DATE,
                            email VARCHAR(255) UNIQUE,
                            phone VARCHAR(50),
                            education VARCHAR(100),
                            status VARCHAR(50),
                            hobbies TEXT,
                            postalcode VARCHAR(20),
                            country VARCHAR(100),
                            state VARCHAR(100),
                            city VARCHAR(100),
                            address TEXT,
                            password VARCHAR(255),
                            photo BYTEA
                        );";
                    
                    using (var command = new Npgsql.NpgsqlCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("[Database] Students table checked/created successfully");
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Database] Error initializing database: {ex.Message}");
                }
            }

            // Use forwarded headers from reverse proxy
            app.UseForwardedHeaders();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHttpsRedirection(); // Only redirect in development
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                // Don't use HTTPS redirection in production - Render handles SSL at load balancer
            }

            app.UseStaticFiles();


            app.UseRouting();

            // Use Authentication & Authorization BEFORE SignalR
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Map SignalR Hubs with increased buffer sizes
                endpoints.MapHub<VideoCallHub>("/videoCallHub", options =>
                {
                    options.ApplicationMaxBufferSize = 1024 * 1024; // 1MB
                    options.TransportMaxBufferSize = 1024 * 1024; // 1MB
                    options.TransportSendTimeout = TimeSpan.FromSeconds(30);
                });
            });
            app.Urls.Add("http://0.0.0.0:5000");

            app.Run();
        }
    }
}