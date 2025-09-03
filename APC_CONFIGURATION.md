# APC Data Collection Configuration

The APC Data Collection Worker supports the following environment variables for configuration:

## Logging Configuration
- `STRUCTURED_LOGS_ENABLED` - Set to "true" for JSON formatted logs (default: false)
- `SERILOG_MINIMUMLEVEL` - Set minimum log level: Debug, Information, Warning, Error (default: Information)
- `SERILOG_OVERRIDE_APCLIENT` - Override log level specifically for ApClient logs (default: Information)

## Connection Settings
- `APC_HOST` - APC server hostname/IP address (default: "10.102.84.2")
- `APC_PORT` - APC server port (default: 8008)

## Collection Settings
- `COLLECTION_INTERVAL_MINUTES` - Minutes between collection cycles (default: 30)
- `MAX_ITERATIONS` - Number of data points to collect per cycle (default: 20)
- `TAG_NAME` - APC tag name to read from (default: "Turbine.System.Logs.TenMinData")
- `START_TIME` - Starting datetime for data collection (default: "2018-12-18T06:00:00")

## Retry and Error Handling
- `MAX_RETRIES` - Maximum number of retry attempts on failure (default: 3)
- `RETRY_DELAY_SECONDS` - Base delay between retry attempts in seconds (default: 30)
  - Uses exponential backoff: delay = RETRY_DELAY_SECONDS * retry_attempt

## Monitoring and Observability
- `OTEL_EXPORTER_OTLP_ENDPOINT` - OpenTelemetry endpoint for metrics export
- `CONNECTION_STRING` - PostgreSQL connection string for data storage

## Log Correlation Features

The service now provides unified logging between the Worker and ApClient components:

### Shared Log Properties
- `Application`: "hackathon-dotnet"
- `Service`: "wtg-apc-collector"
- `Operation`: Current operation (e.g., "WTG-DataCollection")
- `WTG-Host`: Target WTG hostname
- `WTG-Port`: Target WTG port
- `Iteration`: Current iteration number
- `DataTime`: Timestamp being processed
- `RequestType`: Type of APC request
- `TagName`: APC tag being read

### Log Output Format
**Console (Non-structured):**
```
[2024-01-15 10:30:15.123 INF] hackathon_dotnet.Worker: ?? Establishing connection to WTG APC server at 10.102.84.2:8008...
[2024-01-15 10:30:15.234 DBG] ApClient.Client.ApClient: Connecting to 10.102.84.2:8008
[2024-01-15 10:30:15.345 DBG] ApClient.Client.ApClient: Connected to 10.102.84.2:8008
[2024-01-15 10:30:15.456 INF] hackathon_dotnet.Worker: ? Successfully connected to WTG APC server 10.102.84.2:8008 in 123ms
```

**JSON (Structured):**
```json
{
  "@t": "2024-01-15T10:30:15.123Z",
  "@l": "Information",
  "@mt": "?? Establishing connection to WTG APC server at {Host}:{Port}...",
  "Host": "10.102.84.2",
  "Port": 8008,
  "Application": "hackathon-dotnet",
  "Service": "wtg-apc-collector",
  "Operation": "WTG-DataCollection",
  "WTG-Host": "10.102.84.2",
  "WTG-Port": 8008,
  "SourceContext": "hackathon_dotnet.Worker"
}
```

## Available Metrics

The worker exports the following metrics:

### Connection Metrics
- `apc_connection_attempts_total` - Total connection attempts
- `apc_connection_success_total` - Successful connections
- `apc_connection_failure_total` - Failed connections
- `apc_connection_duration_seconds` - Connection duration histogram
- `apc_reconnection_attempts_total` - Reconnection attempts

### Data Collection Metrics
- `apc_reads_successful_total` - Successful data reads
- `apc_reads_failed_total` - Failed data reads
- `apc_read_duration_seconds` - Read operation duration histogram

### Data Processing Metrics
- `parse_success_total` - Successful data parses
- `parse_failure_total` - Failed data parses
- `parse_duration_seconds` - Parse operation duration histogram

### General Metrics
- `api_requests` - API endpoint requests
- `loops` - Worker loop iterations
- `rows_created` - Database rows created

## Example Configuration

```bash
export APC_HOST="10.102.84.2"
export APC_PORT="8008"
export COLLECTION_INTERVAL_MINUTES="15"
export MAX_ITERATIONS="50"
export MAX_RETRIES="5"
export RETRY_DELAY_SECONDS="60"
export STRUCTURED_LOGS_ENABLED="true"
export SERILOG_MINIMUMLEVEL="Information"
export SERILOG_OVERRIDE_APCLIENT="Debug"
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318/v1/metrics"
export CONNECTION_STRING="User ID=hackathon;Password=hackathon;Host=localhost;Port=5432;Database=hackathon_db;"
```

## API Endpoints

- `GET /` - Returns configuration and status information
- `GET /health` - Health check endpoint

## Troubleshooting Logs

### Connection Issues
Look for logs with these patterns:
- `?? WTG Connection Problem` - Connection failures
- `WTG-Operation: Connect` - Connection attempts
- `ApClient.Client.ApClient: Connecting to` - Low-level connection logs

### Data Collection Issues
Look for logs with these patterns:
- `?? Reading WTG ten-minute data` - Data read operations
- `WTG-Operation: ReadFromTime` - Specific read operations
- `RequestType: ReadFromTime` - Request context

### Error Correlation
All related logs will share the same correlation properties, making it easy to trace issues across both components.