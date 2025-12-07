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
            // Fix for Render's inotify limit issue
            Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

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
                    using (var connection = dbService.GetConnectionAsync().Result)
                    {
                        var createTableQueries = @"
                            CREATE TABLE IF NOT EXISTS students (id SERIAL PRIMARY KEY, firstname VARCHAR(100), lastname VARCHAR(100), gender VARCHAR(50), dateofbirth DATE, email VARCHAR(255) UNIQUE, phone VARCHAR(50), education VARCHAR(100), status VARCHAR(50), hobbies TEXT, postalcode VARCHAR(20), country VARCHAR(100), state VARCHAR(100), city VARCHAR(100), address TEXT, password VARCHAR(255), photo BYTEA);
                            
                            CREATE TABLE IF NOT EXISTS messages (
                                id SERIAL PRIMARY KEY,
                                sender_email VARCHAR(255),
                                receiver_email VARCHAR(255),
                                message TEXT,
                                sent_at TIMESTAMP,
                                is_read INT DEFAULT 0,
                                file_path TEXT,
                                image_path TEXT,
                                file_name VARCHAR(255),
                                file_original_name VARCHAR(255),
                                is_call_message BOOLEAN DEFAULT FALSE,
                                call_duration VARCHAR(50),
                                call_status VARCHAR(50)
                            );

                            CREATE TABLE IF NOT EXISTS groups (
                                group_id SERIAL PRIMARY KEY,
                                group_name VARCHAR(255),
                                created_by VARCHAR(255),
                                created_at TIMESTAMP,
                                group_image BYTEA,
                                updated_at TIMESTAMP,
                                last_activity TIMESTAMP
                            );

                            CREATE TABLE IF NOT EXISTS group_members (
                                id SERIAL PRIMARY KEY,
                                group_id INT,
                                student_email VARCHAR(255),
                                joined_at TIMESTAMP
                            );

                            CREATE TABLE IF NOT EXISTS group_messages (
                                id SERIAL PRIMARY KEY,
                                group_id INT,
                                sender_email VARCHAR(255),
                                message TEXT,
                                sent_at TIMESTAMP,
                                is_read INT DEFAULT 0,
                                file_path TEXT,
                                image_path TEXT,
                                file_name VARCHAR(255),
                                file_original_name VARCHAR(255)
                            );
                        ";
                        
                        using (var command = new Npgsql.NpgsqlCommand(createTableQueries, connection))
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine("[Database] All tables (students, messages, groups, etc.) checked/created successfully");
                        }
                    }
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