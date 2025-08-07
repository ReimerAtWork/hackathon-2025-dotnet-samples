using Microsoft.Extensions.Options;
using Npgsql;

namespace Vestas.Spc.Hackathon.DotnetSample.Presentation.Worker;

public class Worker(
    SampleMetrics sampleMetrics,
    ILogger<Worker> logger,
    IOptions<WorkerOptions> options) : BackgroundService
{
    private readonly SampleMetrics _sampleMetrics = sampleMetrics;
    private readonly ILogger<Worker> _logger = logger;
    private readonly WorkerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: [{time}]", DateTimeOffset.Now);

            using var databaseConnection = new NpgsqlConnection(_options.DatabaseConnectionString);
            try
            {
                databaseConnection.Open();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to database with connection string [{connectionString}]", _options.DatabaseConnectionString);

                await Task.Delay(_options.WorkerInterval, stoppingToken);
                continue;
            }

            await Task.Delay(_options.WorkerInterval, stoppingToken);
        }
    }
}
