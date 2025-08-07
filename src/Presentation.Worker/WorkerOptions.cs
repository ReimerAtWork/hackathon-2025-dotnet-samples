namespace Vestas.Spc.Hackathon.DotnetSample.Presentation.Worker;

public record WorkerOptions
{
    public bool AreStructuredLogsEnabled { get; init; } = true;

    public string DatabaseConnectionString { get; init; } = string.Empty;

    public TimeSpan WorkerInterval { get; init; } = TimeSpan.FromMinutes(5);

    public string OtelExporterOtlpEndpoint { get; init; } = string.Empty;
}
