using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using Vestas.Spc.Hackathon.DotnetSample.Presentation.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(nameof(WorkerOptions)));
builder.Services.AddLogging(l =>
{
    if (builder.Configuration.GetValue<bool>("WorkerOptions:AreStructuredLogsEnabled"))
        l.AddJsonConsole();
});

// Metrics
var otelAddress = builder.Configuration.GetValue<string>("WorkerOptions:OtelExporterOtlpEndpoint");
var isMetricsEnabled = string.IsNullOrWhiteSpace(otelAddress) == false;
if (isMetricsEnabled)
{
    builder.Services.AddSingleton<SampleMetrics>();
    var meterProvider = Sdk.CreateMeterProviderBuilder()
        // Other setup code, like setting a resource goes here too
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otelAddress);
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        })
        .AddMeter(SampleMetrics.METER_NAME)
        .AddMeter("System.Runtime")
        .AddMeter("System.Net.Http")
        .Build();
}

var host = builder.Build();
host.Run();
