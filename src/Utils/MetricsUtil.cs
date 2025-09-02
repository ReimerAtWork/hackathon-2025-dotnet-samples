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
        private Meter? _meter;

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
                .Build();

            _meter = new Meter("hackathon-dotnet-sample");
        }

        public void WriteCounter(string name, double value)
        {
            if (_meter == null) return;
            var counter = _meter.CreateCounter<double>(name);
            counter.Add(value);
        }

        public void CleanUpMetrics()
        {
            _meterProvider?.Dispose();
        }
    }
}
