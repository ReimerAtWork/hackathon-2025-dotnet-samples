
using System;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace hackathon_dotnet.Utils
{
    public class DbUtil
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;
        private NpgsqlConnection? _connection;

        public DbUtil(ILogger logger, string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;
        }

        public void Init()
        {
            try
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
                _logger.LogInformation($"Connected to database: {_connection.Database}");
                using var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS public.hackathon (timestamp TEXT);", _connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to connect to database: {Message}", e.Message);
                throw;
            }
        }

        public void WriteRow()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            var timestamp = DateTime.Now.ToString("o");
            using var cmd = new NpgsqlCommand("INSERT INTO public.hackathon (timestamp) VALUES (@p)", _connection);
            cmd.Parameters.AddWithValue("p", timestamp);
            cmd.ExecuteNonQuery();
        }

        public long GetRowCount()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM public.hackathon", _connection);
            return (long)(cmd.ExecuteScalar() ?? 0);
        }
    }
}
