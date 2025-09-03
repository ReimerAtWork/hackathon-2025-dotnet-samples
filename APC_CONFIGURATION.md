# WTG Simulator Data Collection Configuration

The WTG Simulator Data Collection Worker supports the following environment variables for configuration:

## Logging Configuration
- `STRUCTURED_LOGS_ENABLED` - Set to "true" for JSON formatted logs (default: false)
- `SERILOG_MINIMUMLEVEL` - Set minimum log level: Debug, Information, Warning, Error (default: Information)
- `SERILOG_OVERRIDE_APCLIENT` - Override log level specifically for ApClient logs (default: Information)

## Connection Settings
- `APC_HOST` - WTG simulator hostname/IP address (default: "wtgsim1.wl-we-want-to-code-more")
- `APC_PORT` - WTG simulator APC port (default: 8008)

## Collection Settings
- `COLLECTION_INTERVAL_MINUTES` - Minutes between collection cycles (default: 30)
- `MAX_ITERATIONS` - Number of data points to collect per cycle (default: 20)
- `TAG_NAME` - APC tag name to read from (default: "Turbine.System.Logs.TenMinData")
- `START_TIME` - Starting datetime for historical data collection (default: "2018-12-18T06:00:00")

## Retry and Error Handling
- `MAX_RETRIES` - Maximum number of retry attempts on failure (default: 3)
- `RETRY_DELAY_SECONDS` - Base delay between retry attempts in seconds (default: 30)
  - Uses exponential backoff: delay = RETRY_DELAY_SECONDS * retry_attempt

## Monitoring and Observability
- `OTEL_EXPORTER_OTLP_ENDPOINT` - OpenTelemetry endpoint for metrics export
- `CONNECTION_STRING` - PostgreSQL connection string for data storage

## WTG Simulator Connection

The service connects to the WTG (Wind Turbine Generator) simulator to collect historical 10-minute data:

### Default Configuration
- **Host**: `wtgsim1.wl-we-want-to-code-more`
- **Port**: `8008`
- **Protocol**: APC (Automatic Process Control) over TCP
- **Data**: Historical turbine logs from December 18, 2018

### Data Collection Process
1. **Connection**: Establishes TCP connection to WTG simulator APC service
2. **Authentication**: Uses anonymous connection (no login required for simulator)
3. **Data Reading**: Requests 10-minute interval data starting from configured time
4. **Parsing**: Processes XML-formatted turbine data
5. **Storage**: Optionally stores parsed data in PostgreSQL database
6. **Metrics**: Exports telemetry data via OpenTelemetry

## Log Correlation Features

The service provides unified logging between the Worker and ApClient components:

### Shared Log Properties
- `Application`: "hackathon-dotnet"
- `Service`: "wtg-apc-collector"
- `Operation`: Current operation (e.g., "WTG-DataCollection")
- `WTG-Host`: Target WTG simulator hostname
- `WTG-Port`: Target WTG simulator port
- `Iteration`: Current iteration number
- `DataTime`: Timestamp being processed
- `RequestType`: Type of APC request
- `TagName`: APC tag being read

### Log Output Examples

**WTG Simulator Connection:**
```
[2024-01-15 10:30:15.123 INF] hackathon_dotnet.Worker: ?? Establishing connection to WTG simulator at wtgsim1.wl-we-want-to-code-more:8008...
[2024-01-15 10:30:15.234 INF] ApClient.Client.ApClient: Connecting to wtgsim1.wl-we-want-to-code-more:8008
[2024-01-15 10:30:15.345 DBG] ApClient.Client.ApClient: Connected to wtgsim1.wl-we-want-to-code-more:8008
[2024-01-15 10:30:15.456 INF] hackathon_dotnet.Worker: ? Successfully connected to WTG simulator wtgsim1.wl-we-want-to-code-more:8008 in 123ms
```

**Historical Data Collection:**
```
[2024-01-15 10:30:16.123 INF] hackathon_dotnet.Worker: ?? Starting WTG historical data collection from 2018-12-18 06:00:00 for 20 iterations (10-min intervals)
[2024-01-15 10:30:16.234 DBG] hackathon_dotnet.Worker: ?? Reading WTG ten-minute data for 2018-12-18 06:00:00 (iteration 1/20)
[2024-01-15 10:30:16.345 DBG] ApClient.Client.ApClient: Creating Read SOAP message with tag: Turbine.System.Logs.TenMinData
[2024-01-15 10:30:16.456 INF] hackathon_dotnet.Worker: ?? Successfully parsed WTG data for 2018-12-18 06:00:00. Records: 145, Duration: 25ms
```

## Available Metrics

### Connection Metrics
- `apc_connection_attempts_total` - Total WTG simulator connection attempts
- `apc_connection_success_total` - Successful WTG connections
- `apc_connection_failure_total` - Failed WTG connections
- `apc_connection_duration_seconds` - Connection duration histogram
- `apc_reconnection_attempts_total` - Reconnection attempts

### Data Collection Metrics
- `apc_reads_successful_total` - Successful data reads from WTG simulator
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
# WTG Simulator Connection
export APC_HOST="wtgsim1.wl-we-want-to-code-more"
export APC_PORT="8008"

# Data Collection Settings
export COLLECTION_INTERVAL_MINUTES="15"
export MAX_ITERATIONS="50"
export START_TIME="2018-12-18T06:00:00"

# Error Handling
export MAX_RETRIES="5"
export RETRY_DELAY_SECONDS="60"

# Observability
export STRUCTURED_LOGS_ENABLED="true"
export SERILOG_MINIMUMLEVEL="Information"
export SERILOG_OVERRIDE_APCLIENT="Debug"
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318/v1/metrics"
export CONNECTION_STRING="User ID=hackathon;Password=hackathon;Host=localhost;Port=5432;Database=hackathon_db;"
```

## API Endpoints

- `GET /` - Returns WTG simulator connection status and configuration
- `GET /health` - Health check endpoint with service uptime

### API Response Example

```json
{
  "message": "WTG Simulator Data Collection Worker API",
  "status": "running",
  "time": "2024-01-15T10:30:15.123Z",
  "configuration": {
    "wtgSimulatorHost": "wtgsim1.wl-we-want-to-code-more",
    "wtgSimulatorPort": 8008,
    "collectionIntervalMinutes": 30,
    "maxIterationsPerCycle": 20,
    "maxRetryAttempts": 3,
    "retryDelaySeconds": 30,
    "tagName": "Turbine.System.Logs.TenMinData",
    "historicalDataStartTime": "2018-12-18T06:00:00",
    "dataPointIntervalMinutes": 10
  },
  "targets": {
    "wtgSimulator": "wtgsim1.wl-we-want-to-code-more:8008",
    "dataTag": "Turbine.System.Logs.TenMinData",
    "timeRange": "2018-12-18 06:00 + 200 minutes"
  }
}
```

## Troubleshooting

### WTG Simulator Connection Issues
Look for logs with these patterns:
- `?? WTG Connection Problem` - Connection failures
- `WTG-Operation: Connect` - Connection attempts
- `ApClient.Client.ApClient: Connecting to` - Low-level connection logs

**Common Issues:**
1. **Network connectivity**: Verify WTG simulator hostname resolves
2. **Service availability**: Check if APC service is running on port 8008
3. **Firewall**: Ensure port 8008 is accessible

### Data Collection Issues
Look for logs with these patterns:
- `?? Reading WTG ten-minute data` - Data read operations
- `WTG-Operation: ReadFromTime` - Specific read operations
- `RequestType: ReadFromTime` - Request context

**Common Issues:**
1. **Tag not found**: Verify "Turbine.System.Logs.TenMinData" exists in simulator
2. **No historical data**: Check if simulator has data for the requested time range
3. **Timeout issues**: Consider increasing ResponseReadTimeout

### Error Correlation
All related logs share the same correlation properties:
- `WTG-Host` and `WTG-Port` for connection context
- `Iteration` and `DataTime` for specific data point context
- `Operation` for grouping related activities