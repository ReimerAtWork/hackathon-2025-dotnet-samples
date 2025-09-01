using hackathon_dotnet.Utils;

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
            _logger.LogInformation("Starting...");

            _metrics?.Init();
            _db?.Init();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting worker loop");
                if (_metrics != null)
                {
                    _logger.LogInformation("Writing metrics..");
                    _metrics.WriteCounter("loops", 1);
                }
                else
                {
                    _logger.LogWarning("Skipping metrics, because they are not configured.");
                }

                if (_db != null)
                {
                    _logger.LogInformation("Interacting with database..");
                    try
                    {
                        _db.WriteRow();
                        var rowCount = _db.GetRowCount();
                        _metrics?.WriteCounter("rows_created", 1);
                        _logger.LogInformation($"There are {rowCount} rows in the database");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to interact with database: {Message}", e.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Skipping database interaction, because it is not configured.");
                }

                _logger.LogInformation("Worker loop finished, will run again in 15 seconds");
                await Task.Delay(15000, stoppingToken);
            }

            _metrics?.CleanUpMetrics();
        }
    }
}