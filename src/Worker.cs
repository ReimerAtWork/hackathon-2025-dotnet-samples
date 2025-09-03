using ApClient;
using ApClient.Client;
using hackathon_dotnet.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using System.Xml;

namespace hackathon_dotnet
{
    public class Worker : BackgroundService
    {
        #region Configuration
        // APC Connection Configuration
        private const string APC_HOST = "localhost";
        private const int APC_PORT = 8008;
        
        // Data Collection Configuration
        private const int COLLECTION_INTERVAL_MINUTES = 30;
        private const int MAX_ITERATIONS = 20;
        private const string TAG_NAME = "Turbine.System.Logs.TenMinData";
        private const int MAX_ROWS = 5;
        private static readonly DateTime START_TIME = new DateTime(2018, 12, 18, 6, 0, 0);
        
        // Retry and Error Handling Configuration
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_SECONDS = 30;
        private const int ITERATION_DELAY_MS = 1000;
        
        // Timing Configuration
        private const int DATA_POINT_INTERVAL_MINUTES = 10;
        private const int ERROR_RECOVERY_DELAY_MINUTES = 1;
        #endregion

        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly DbUtil? _db;
        private readonly MetricsUtil? _metrics;
        private readonly Meter _workerMeter;
        
        // Data collection metrics
        private readonly Counter<long> _connectionAttemptsCounter;
        private readonly Counter<long> _connectionSuccessCounter;
        private readonly Counter<long> _connectionFailureCounter;
        private readonly Counter<long> _successfulReadsCounter;
        private readonly Counter<long> _failedReadsCounter;
        private readonly Counter<long> _parseSuccessCounter;
        private readonly Counter<long> _parseFailureCounter;
        private readonly Counter<long> _reconnectionAttemptsCounter;
        private readonly Histogram<double> _readDurationHistogram;
        private readonly Histogram<double> _parseDurationHistogram;
        private readonly Histogram<double> _connectionDurationHistogram;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
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

            // Initialize worker-specific metrics
            _workerMeter = new Meter("hackathon-dotnet.apc-worker");
            _connectionAttemptsCounter = _workerMeter.CreateCounter<long>("apc_connection_attempts_total", "Total number of APC connection attempts");
            _connectionSuccessCounter = _workerMeter.CreateCounter<long>("apc_connection_success_total", "Total number of successful APC connections");
            _connectionFailureCounter = _workerMeter.CreateCounter<long>("apc_connection_failure_total", "Total number of failed APC connections");
            _successfulReadsCounter = _workerMeter.CreateCounter<long>("apc_reads_successful_total", "Total number of successful APC reads");
            _failedReadsCounter = _workerMeter.CreateCounter<long>("apc_reads_failed_total", "Total number of failed APC reads");
            _parseSuccessCounter = _workerMeter.CreateCounter<long>("parse_success_total", "Total number of successful parses");
            _parseFailureCounter = _workerMeter.CreateCounter<long>("parse_failure_total", "Total number of failed parses");
            _reconnectionAttemptsCounter = _workerMeter.CreateCounter<long>("apc_reconnection_attempts_total", "Total number of APC reconnection attempts");
            _readDurationHistogram = _workerMeter.CreateHistogram<double>("apc_read_duration_seconds", "Duration of APC read operations in seconds");
            _parseDurationHistogram = _workerMeter.CreateHistogram<double>("parse_duration_seconds", "Duration of parse operations in seconds");
            _connectionDurationHistogram = _workerMeter.CreateHistogram<double>("apc_connection_duration_seconds", "Duration of APC connection operations in seconds");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initialize metrics and database if available
            _metrics?.Init();
            _db?.Init();

            _logger.LogInformation("🚀 Starting WTG APC Data Collection Worker");
            _logger.LogInformation("⚙️ Configuration: WTG Host={Host}:{Port}, Collection Interval={IntervalMinutes}min, " +
                "Iterations per cycle={MaxIterations}, Max retry attempts={MaxRetries}", 
                APC_HOST, APC_PORT, COLLECTION_INTERVAL_MINUTES, MAX_ITERATIONS, MAX_RETRIES);

            // Start web API in background
            var apiTask = StartWebApi(stoppingToken);
            
            // Start data collection loop
            var dataCollectionTask = RunDataCollectionLoop(stoppingToken);

            _logger.LogInformation("🌐 Web API and WTG data collection services started concurrently");

            // Wait for either task to complete (or cancellation)
            await Task.WhenAny(apiTask, dataCollectionTask);
        }

        private async Task StartWebApi(CancellationToken stoppingToken)
        {
            try
            {
                var builder = WebApplication.CreateBuilder();
                var app = builder.Build();

                app.MapGet("/", () => 
                {
                    _metrics?.IncrementApiRequests();
                    _logger.LogInformation("🌐 API status request received");
                    
                    return Results.Json(new { 
                        message = "WTG APC Data Collection Worker API", 
                        status = "running",
                        time = DateTime.UtcNow,
                        configuration = new {
                            wtgHost = APC_HOST,
                            wtgPort = APC_PORT,
                            collectionIntervalMinutes = COLLECTION_INTERVAL_MINUTES,
                            maxIterationsPerCycle = MAX_ITERATIONS,
                            maxRetryAttempts = MAX_RETRIES,
                            retryDelaySeconds = RETRY_DELAY_SECONDS,
                            tagName = TAG_NAME,
                            dataStartTime = START_TIME
                        }
                    });
                });

                app.MapGet("/health", () => 
                {
                    _logger.LogDebug("💚 Health check request received");
                    return Results.Ok(new { 
                        status = "healthy", 
                        service = "wtg-apc-collector",
                        timestamp = DateTime.UtcNow,
                        uptime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime)
                    });
                });

                _logger.LogInformation("🌐 Starting WTG data collection Web API...");
                await app.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🌐 Failed to start WTG data collection Web API - {ErrorMessage}", ex.Message);
                System.Console.WriteLine($"🌐 Web API Problem: {ex.GetType().Name} - {ex.Message}");
            }
        }

        private async Task RunDataCollectionLoop(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔄 WTG data collection loop started - will run every {IntervalMinutes} minutes", COLLECTION_INTERVAL_MINUTES);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🔍 Starting WTG data collection cycle...");
                    await CollectDataWithRetry(stoppingToken);
                    
                    var delay = TimeSpan.FromMinutes(COLLECTION_INTERVAL_MINUTES);
                    _logger.LogInformation("✅ WTG data collection cycle completed successfully. " +
                        "Next collection in {DelayMinutes} minutes at {NextRun:HH:mm:ss}", 
                        delay.TotalMinutes, DateTime.Now.Add(delay));
                    
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("🛑 WTG data collection cancelled by shutdown request");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "⚠️ Unexpected error in WTG data collection loop - {ErrorMessage}. " +
                        "Will retry in {DelayMinutes} minute(s)", ex.Message, ERROR_RECOVERY_DELAY_MINUTES);
                    
                    System.Console.WriteLine($"⚠️ WTG System Error: {ex.GetType().Name} - Recovery attempt in {ERROR_RECOVERY_DELAY_MINUTES} minute(s)");
                    
                    await Task.Delay(TimeSpan.FromMinutes(ERROR_RECOVERY_DELAY_MINUTES), stoppingToken);
                }
            }
            
            _logger.LogInformation("🏁 WTG data collection loop stopped");
        }

        private async Task CollectDataWithRetry(CancellationToken stoppingToken)
        {
            var retryCount = 0;
            
            while (retryCount <= MAX_RETRIES && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectDataAsync(stoppingToken);
                    return; // Success, exit retry loop
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _reconnectionAttemptsCounter.Add(1);
                    
                    if (retryCount > MAX_RETRIES)
                    {
                        _logger.LogError(ex, "💥 WTG data collection failed permanently after {MaxRetries} attempts. " +
                            "Manual intervention required - check WTG status, network connectivity, and APC service health.", MAX_RETRIES);
                        
                        System.Console.WriteLine($"💥 WTG Connection Problem: All {MAX_RETRIES} connection attempts failed. WTG may be offline or unreachable.");
                        throw;
                    }
                    
                    var delaySeconds = RETRY_DELAY_SECONDS * retryCount; // Exponential backoff
                    var errorType = ex.GetType().Name;
                    
                    _logger.LogWarning(ex, "🔄 WTG connection attempt {RetryCount}/{MaxRetries} failed ({ErrorType}). " +
                        "Retrying in {DelaySeconds} seconds... (Exponential backoff strategy)", 
                        retryCount, MAX_RETRIES, errorType, delaySeconds);
                    
                    System.Console.WriteLine($"🔄 WTG Connection Problem: Attempt {retryCount}/{MAX_RETRIES} failed. Retrying in {delaySeconds}s...");
                    
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                }
            }
        }

        private async Task CollectDataAsync(CancellationToken stoppingToken)
        {
            ApClient.Client.ApClient? client = null;
            var parser = new TenMinParser();

            try
            {
                // Add log context for correlation between Worker and ApClient logs
                using (Serilog.Context.LogContext.PushProperty("Operation", "WTG-DataCollection"))
                using (Serilog.Context.LogContext.PushProperty("WTG-Host", APC_HOST))
                using (Serilog.Context.LogContext.PushProperty("WTG-Port", APC_PORT))
                {
                    client = new ApClient.Client.ApClient();
                    
                    _logger.LogInformation("🔌 Establishing connection to WTG APC server at {Host}:{Port}...", APC_HOST, APC_PORT);
                    _connectionAttemptsCounter.Add(1);
                    
                    var connectStart = DateTime.UtcNow;
                    await client.ConnectAsync(APC_HOST, APC_PORT);
                    var connectDuration = DateTime.UtcNow - connectStart;
                    
                    _connectionDurationHistogram.Record(connectDuration.TotalSeconds);
                    _connectionSuccessCounter.Add(1);
                    
                    _logger.LogInformation("✅ Successfully connected to WTG APC server {Host}:{Port} in {Duration}ms", 
                        APC_HOST, APC_PORT, connectDuration.TotalMilliseconds);

                    var currentTime = START_TIME;
                    
                    _logger.LogInformation("🔄 Starting WTG data collection from {StartTime} for {MaxIterations} iterations", currentTime, MAX_ITERATIONS);

                    var successfulIterations = 0;
                    for (int i = 0; i < MAX_ITERATIONS && !stoppingToken.IsCancellationRequested; i++)
                    {
                        // Add iteration context for each data point
                        using (Serilog.Context.LogContext.PushProperty("Iteration", i + 1))
                        using (Serilog.Context.LogContext.PushProperty("DataTime", currentTime))
                        {
                            try
                            {
                                await ProcessSingleDataPoint(client, parser, currentTime, i + 1, stoppingToken);
                                successfulIterations++;
                                
                                // Increment loops counter for existing metrics
                                _metrics?.IncrementLoops();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "❌ WTG data collection failed for timestamp {Time} (iteration {Iteration}/{MaxIterations}) - {ErrorMessage}", 
                                    currentTime, i + 1, MAX_ITERATIONS, ex.Message);
                                _failedReadsCounter.Add(1);
                                
                                // Log to console as in original code with more context
                                System.Console.WriteLine($"❌ WTG data collection failed on {currentTime:yyyy-MM-dd HH:mm:ss} - {ex.GetType().Name}: {ex.Message}");
                            }
                            finally
                            {
                                // Always advance time, even if iteration failed
                                currentTime = currentTime.AddMinutes(DATA_POINT_INTERVAL_MINUTES);
                            }
                        }
                        
                        // Small delay between iterations to avoid overwhelming the server
                        if (i < MAX_ITERATIONS - 1)
                        {
                            await Task.Delay(ITERATION_DELAY_MS, stoppingToken);
                        }
                    }

                    _logger.LogInformation("✅ WTG data collection cycle completed. Successfully processed {SuccessfulIterations}/{MaxIterations} iterations ({SuccessRate:P1})", 
                        successfulIterations, MAX_ITERATIONS, (double)successfulIterations / MAX_ITERATIONS);
                }
            }
            catch (Exception ex)
            {
                _connectionFailureCounter.Add(1);
                
                // More detailed connection error logging
                var errorType = ex.GetType().Name;
                var errorDetails = ex switch
                {
                    SocketException sockEx => $"Network connectivity issue (Code: {sockEx.SocketErrorCode})",
                    TimeoutException => "Connection timeout - WTG may be unreachable or overloaded",
                    InvalidOperationException => "WTG APC service configuration issue",
                    _ => $"Unexpected connection error ({errorType})"
                };
                
                _logger.LogError(ex, "🔌 WTG APC server connection failed at {Host}:{Port} - {ErrorDetails}. " +
                    "Check network connectivity, WTG status, and APC service availability.", 
                    APC_HOST, APC_PORT, errorDetails);
                    
                // Console output for immediate visibility
                System.Console.WriteLine($"🔌 WTG Connection Problem: Unable to connect to {APC_HOST}:{APC_PORT} - {errorDetails}");
                
                throw;
            }
            finally
            {
                try
                {
                    client?.Dispose();
                    _logger.LogDebug("🔌 WTG APC client connection closed and disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Issue while closing WTG APC client connection - {ErrorMessage}", ex.Message);
                }
            }
        }

        private async Task ProcessSingleDataPoint(ApClient.Client.ApClient client, TenMinParser parser, DateTime time, int iteration, CancellationToken stoppingToken)
        {
            var readStart = DateTime.UtcNow;
            
            try
            {
                // Add request context for ApClient operations
                using (Serilog.Context.LogContext.PushProperty("RequestType", "ReadFromTime"))
                using (Serilog.Context.LogContext.PushProperty("TagName", TAG_NAME))
                {
                    _logger.LogDebug("📊 Reading WTG ten-minute data for {Time} (iteration {Iteration}/{MaxIterations})", 
                        time, iteration, MAX_ITERATIONS);
                    
                    var data = await client.ReadFromTimeAsync(TAG_NAME, time, MAX_ROWS);
                    var readDuration = DateTime.UtcNow - readStart;
                    
                    _readDurationHistogram.Record(readDuration.TotalSeconds);
                    _successfulReadsCounter.Add(1);
                    
                    _logger.LogDebug("✅ Successfully read WTG data for {Time} in {Duration}ms (Tag: {TagName})", 
                        time, readDuration.TotalMilliseconds, TAG_NAME);

                    // Parse the data
                    if (data.Value != null)
                    {
                        await ParseData(parser, data.Value.ToString(), time);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Received null data from WTG for timestamp {Time} - Tag '{TagName}' may not exist or have no data", 
                            time, TAG_NAME);
                    }
                }
            }
            catch (Exception ex)
            {
                var readDuration = DateTime.UtcNow - readStart;
                _readDurationHistogram.Record(readDuration.TotalSeconds);
                _failedReadsCounter.Add(1);
                
                var errorDetails = ex switch
                {
                    TimeoutException => "WTG response timeout - system may be busy or unresponsive",
                    InvalidOperationException when ex.Message.Contains("timeout") => "WTG read operation timed out",
                    InvalidOperationException when ex.Message.Contains("request id") => "WTG response synchronization issue",
                    _ => $"WTG communication error ({ex.GetType().Name})"
                };
                
                _logger.LogError(ex, "❌ Failed to read WTG data for {Time} (iteration {Iteration}) after {Duration}ms - {ErrorDetails}", 
                    time, iteration, readDuration.TotalMilliseconds, errorDetails);
                throw;
            }
        }

        private async Task ParseData(TenMinParser parser, string data, DateTime time)
        {
            var parseStart = DateTime.UtcNow;
            
            try
            {
                var result = parser.Parse(data);
                var parseDuration = DateTime.UtcNow - parseStart;
                
                _parseDurationHistogram.Record(parseDuration.TotalSeconds);
                _parseSuccessCounter.Add(1);
                
                _logger.LogInformation("📈 Successfully parsed WTG data for {Time}. Records: {RowCount}, Duration: {Duration}ms", 
                    time, result.Rows.Count, parseDuration.TotalMilliseconds);
                    
                // Store in database if available
                if (_db != null)
                {
                    try
                    {
                        _db.WriteRow();
                        _metrics?.IncrementRowsCreated();
                        _logger.LogDebug("💾 WTG data stored in database for {Time}", time);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "💾 Database storage failed for WTG data at {Time} - {ErrorMessage}", 
                            time, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                var parseDuration = DateTime.UtcNow - parseStart;
                _parseDurationHistogram.Record(parseDuration.TotalSeconds);
                _parseFailureCounter.Add(1);
                
                var errorDetails = ex switch
                {
                    TenMinParserException parseEx => $"WTG data format issue: {parseEx.Message}",
                    XmlException => "WTG data XML format is invalid or corrupted",
                    ArgumentException => "WTG data structure is unexpected or incomplete",
                    _ => $"WTG data parsing error ({ex.GetType().Name})"
                };
                
                _logger.LogError(ex, "🔍 Failed to parse WTG data for {Time} after {Duration}ms - {ErrorDetails}", 
                    time, parseDuration.TotalMilliseconds, errorDetails);
                throw; // Re-throw to match original behavior
            }
        }

        public override void Dispose()
        {
            _workerMeter?.Dispose();
            _metrics?.CleanUpMetrics();
            base.Dispose();
        }
    }
}