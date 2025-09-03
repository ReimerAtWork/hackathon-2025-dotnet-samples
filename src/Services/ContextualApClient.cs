using ApClient.Client;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace hackathon_dotnet.Services
{
    /// <summary>
    /// Wrapper around ApClient that adds logging context and correlation
    /// </summary>
    public class ContextualApClient : IDisposable
    {
        private readonly ApClient.Client.ApClient _client;
        private readonly ILogger<ContextualApClient> _logger;
        private readonly string _host;
        private readonly int _port;

        public ContextualApClient(ILogger<ContextualApClient> logger, string host, int port)
        {
            _client = new ApClient.Client.ApClient();
            _logger = logger;
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync()
        {
            using (LogContext.PushProperty("WTG-Operation", "Connect"))
            using (LogContext.PushProperty("WTG-Host", _host))
            using (LogContext.PushProperty("WTG-Port", _port))
            {
                _logger.LogInformation("🔌 Connecting to WTG APC server...");
                await _client.ConnectAsync(_host, _port);
                _logger.LogInformation("✅ WTG APC connection established");
            }
        }

        public async Task<ApClient.Client.ApClient.ApValue> ReadFromTimeAsync(string tag, DateTime fromTime, int maxRows)
        {
            using (LogContext.PushProperty("WTG-Operation", "ReadFromTime"))
            using (LogContext.PushProperty("WTG-Tag", tag))
            using (LogContext.PushProperty("WTG-FromTime", fromTime))
            using (LogContext.PushProperty("WTG-MaxRows", maxRows))
            {
                _logger.LogDebug("📊 Reading WTG data from time: {FromTime}", fromTime);
                var result = await _client.ReadFromTimeAsync(tag, fromTime, maxRows);
                _logger.LogDebug("✅ WTG data read completed");
                return result;
            }
        }

        public void Dispose()
        {
            using (LogContext.PushProperty("WTG-Operation", "Dispose"))
            {
                _logger.LogDebug("🔌 Disposing WTG APC client");
                _client?.Dispose();
            }
        }
    }
}