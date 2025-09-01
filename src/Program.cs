using hackathon_dotnet;
using Serilog;
using Serilog.Formatting.Json;

var builder = Host.CreateApplicationBuilder(args);

var structuredLogsEnabled = Environment.GetEnvironmentVariable("STRUCTURED_LOGS_ENABLED")?.ToLower() == "true";

var loggerConfiguration = new LoggerConfiguration();

if (structuredLogsEnabled)
{
    loggerConfiguration.WriteTo.Console(new JsonFormatter());
}
else
{
    loggerConfiguration.WriteTo.Console();
}

var logger = loggerConfiguration.CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
