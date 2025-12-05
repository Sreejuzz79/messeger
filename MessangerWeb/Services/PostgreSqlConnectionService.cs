// Services/PostgreSqlConnectionService.cs
using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace MessangerWeb.Services
{
    public class PostgreSqlConnectionService
    {
        private readonly string _connectionString;
        
        public PostgreSqlConnectionService(IConfiguration configuration)
        {
            var rawConnectionString = configuration.GetConnectionString("DefaultConnection");
            
            Console.WriteLine($"[PostgreSqlConnectionService] Raw connection string: '{rawConnectionString}'");
            
            // Convert Render's postgres:// URL format to Npgsql format if needed
            _connectionString = ConvertConnectionString(rawConnectionString);
            
            Console.WriteLine($"[PostgreSqlConnectionService] Converted connection string: '{_connectionString}'");
        }
        
        private string ConvertConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            // If it's already in Npgsql format (contains "Host="), return as-is
            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                return connectionString;
            }

            // If it starts with postgres:// or postgresql://, convert it
            if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(connectionString);
                    var userInfo = uri.UserInfo.Split(':');
                    
                    var builder = new NpgsqlConnectionStringBuilder
                    {
                        Host = uri.Host,
                        Port = uri.Port > 0 ? uri.Port : 5432,
                        Database = uri.AbsolutePath.TrimStart('/'),
                        Username = userInfo.Length > 0 ? userInfo[0] : "",
                        Password = userInfo.Length > 1 ? userInfo[1] : "",
                        SslMode = SslMode.Require, // Render requires SSL
                        TrustServerCertificate = true // For Render's SSL certificate
                    };

                    return builder.ToString();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to parse PostgreSQL connection string: {ex.Message}");
                }
            }

            // If we can't recognize the format, return as-is and let Npgsql try to parse it
            return connectionString;
        }
        
        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
