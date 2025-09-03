using hackathon_dotnet;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var structuredLogsEnabled = Environment.GetEnvironmentVariable("STRUCTURED_LOGS_ENABLED")?.ToLower() == "true";

// Configure Serilog with enrichment for better log correlation
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("ApClient", Serilog.Events.LogEventLevel.Information) // Control ApClient log level
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "hackathon-dotnet")
    .Enrich.WithProperty("Service", "wtg-apc-collector");

if (structuredLogsEnabled)
{
    loggerConfiguration.WriteTo.Console(outputTemplate: 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} {Operation} {WTG-Host}:{WTG-Port} {Message:lj}{NewLine}{Exception}");
}
else
{
    loggerConfiguration.WriteTo.Console(outputTemplate: 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
}

var serilogLogger = loggerConfiguration.CreateLogger();

// Set Serilog as the global logger for both projects
Log.Logger = serilogLogger;

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(serilogLogger);

// Register IConfiguration for dependency injection
builder.Services.AddSingleton(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

Log.Information("?? Starting Hackathon WTG Data Collection Service");
Log.Information("Configuration: Structured Logs: {StructuredLogs}", structuredLogsEnabled);

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "?? Application terminated unexpectedly");
}
finally
{
    Log.Information("?? Application shutdown completed");
    Log.CloseAndFlush();
}
