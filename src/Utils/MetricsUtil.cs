using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace hackathon_dotnet.Utils
{
    public class MetricsUtil(string endpoint)
    {
        private MeterProvider? _meterProvider;
        public Meter? _meter;
        
        // Pre-defined counters
        private Counter<double>? _apiRequestsCounter;
        private Counter<double>? _loopsCounter;
        private Counter<double>? _rowsCreatedCounter;

        public void Init()
        {
            var resource = ResourceBuilder.CreateDefault()
                .AddService("hackathon-dotnet-sample", serviceInstanceId: "hackathon-dotnet-sample");

            _meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resource)
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(endpoint);
                    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                })
                .AddMeter("motth")
                //.AddMeter("hackathon-dotnet.apc-worker") // Add APC worker-specific meter
                .Build();

            _meter = new Meter("motth");

            // Create all counters upfront
            //_apiRequestsCounter = _meter.CreateCounter<double>("api_requests", description: "Number of API requests received");
            //_loopsCounter = _meter.CreateCounter<double>("loops", description: "Number of worker loops executed");
            //_rowsCreatedCounter = _meter.CreateCounter<double>("rows_created", description: "Number of database rows created");
        }

        //public void IncrementApiRequests(double value = 1)
        //{
        //    _apiRequestsCounter?.Add(value);
        //}

        //public void IncrementLoops(double value = 1)
        //{
        //    _loopsCounter?.Add(value);
        //}

        //public void IncrementRowsCreated(double value = 1)
        //{
        //    _rowsCreatedCounter?.Add(value);
        //}

        public void CleanUpMetrics()
        {
            _meter?.Dispose();
            _meterProvider?.Dispose();
        }
    }
}
