using hackathon_dotnet.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace hackathon_dotnet
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DbUtil? _db;
        private readonly MetricsUtil? _metrics;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            var otelEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogInformation("CONNECTION_STRING set. Starting database setup.");
                _db = new DbUtil(_logger, connectionString);
            }
            else
            {
                _logger.LogWarning("No CONNECTION_STRING set. Database disabled.");
            }

            if (!string.IsNullOrWhiteSpace(otelEndpoint))
            {
                _logger.LogInformation("OTEL_EXPORTER_OTLP_ENDPOINT is set. Metrics enabled.");
                _metrics = new MetricsUtil(otelEndpoint);
            }
            else
            {
                _logger.LogWarning("No OTEL_EXPORTER_OTLP_ENDPOINT set. Metrics disabled.");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var builder = WebApplication.CreateBuilder();
            
            var app = builder.Build();

            // Initialize metrics if available
            _metrics?.Init();

            app.MapGet("/", () => 
            {
                // Count API requests using specific method
                _metrics?.IncrementApiRequests();
                _logger.LogInformation("API request received");
                
                return Results.Json(new { message = "Hello from Worker API", time = DateTime.UtcNow });
            });

            _logger.LogInformation("Starting Web API...");

            await app.RunAsync(stoppingToken);
        }
    }
}