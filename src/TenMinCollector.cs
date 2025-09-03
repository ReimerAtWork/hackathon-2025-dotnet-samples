using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApClient.Client;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace ApClient.Console
{
    public class TenMinCollector
    {
        private readonly TenMinPollConfig _tenMinPollConfig;
        private readonly CheckmarkRepository _checkmarkRepository;
        private Task _pollerTask;

        public TenMinCollector(TenMinPollConfig tenMinPollConfig, CheckmarkRepository checkmarkRepository)
        {
            _tenMinPollConfig = tenMinPollConfig;
            _checkmarkRepository = checkmarkRepository;
        }

        public void Start(CancellationToken cancellationToken)
        {
            _pollerTask = Task.Run(() => PollerTask(cancellationToken), cancellationToken);
        }

        private async Task PollerTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (LogContext.PushProperty("UnitId", _tenMinPollConfig.UnitId))
                using (LogContext.PushProperty("RemoteHost", _tenMinPollConfig.Hostname))
                {
                    try
                    {
                        using (var c = new Client.ApClient(new MessageExchange()))
                        {
                            Log.Debug("Connecting AP");
                            await c.ConnectAsync(_tenMinPollConfig.Hostname, _tenMinPollConfig.Port);

                            Log.Debug("Logging on to AP");
                            await c.Logon(_tenMinPollConfig.Username, _tenMinPollConfig.Password);

                            Log.Information("Connected to turbine");

                            await RunPollLoop(cancellationToken, c);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Something bad happened");

                        //If this keeps happemning we might need to bail
                    }

                    Log.Debug("Sleeping for ten");
                    await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
                }
            }
        }

        private async Task RunPollLoop(CancellationToken cancellationToken, Client.ApClient c)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                DateTime checkMark = _checkmarkRepository.GetCheckmark(_tenMinPollConfig.UnitId);
                Log.Debug("Loaded check-mark: {checkmark}", checkMark);

                var result = await c.ReadFromTimeAsync("Turbine.System.Logs.TenMinData", checkMark, 100);

                if (result == null)
                {
                    Log.Information("No more data for now");
                    break;
                }

                var tenMinData = new TenMinParser().Parse((string)result.Value);

                foreach (var td in tenMinData.Rows)
                {
                    string filePath =
                        $"c:\\temp\\10mindata\\{_tenMinPollConfig.UnitId}\\{td.DateTime:yyyyMMdd-HHmm}.json";

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(td));
                }

                Log.Debug("Read and parsed {rowcound} rows", tenMinData.Rows);

                //Get max ts and add  one minute.
                //There might be a better way, index does not seem to work.
                checkMark = tenMinData.Rows.Select(r => r.DateTime).Max();
                checkMark = checkMark.AddMinutes(1);

                Log.Debug("Save check-mark: {checkmark}", checkMark);
                _checkmarkRepository.SetCheckmark(_tenMinPollConfig.UnitId, checkMark);
            }
        }
    }
}