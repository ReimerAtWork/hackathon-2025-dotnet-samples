using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApClient.Http;
using ApClient.Soap;
using ApClient.Soap.Messages;
using Serilog;
using Serilog.Context;
using HttpRequest = ApClient.Http.HttpRequest;

namespace ApClient.Client
{
    public partial class ApClient : IDisposable
    {
        public static bool PatchItemNamesOnReadReply { get; set; } = true;

        private readonly IMessageExchange _soapMessageExchange;
        private TcpClient _tcpClient;
        private object _debugInfo;
        private Task _readerTask;

        private readonly IDictionary<string, TaskCompletionSource<IReplyItem>> _readCompletionMap = new ConcurrentDictionary<string, TaskCompletionSource<IReplyItem>>();
        private CancellationTokenSource _readerCancelToken;
        private HttpStream _httpStream;

        public ApClient(IMessageExchange soapMessageExchange)
        {
            _soapMessageExchange = soapMessageExchange;
        }

        public ApClient() : this(new MessageExchange())
        {
        }

        public IMessageIdGenerator MessageIdGenerator { get; set; } = new GuidMessageIdGenerator();

        public async Task ConnectAsync(string host, int port)
        {
            if (_tcpClient != null)
            {
                throw new InvalidOperationException("Already connected");
            }

            _tcpClient = new TcpClient();
            Log.Information("Connecting to {host}:{port}", host, port);
            await _tcpClient.ConnectAsync(host, port);
            Log.Debug("Connected to {host}:{port}", host, port);

            _httpStream = new HttpStream(_tcpClient.GetStream());
            StartApReadMessagePump();

            _debugInfo = new
            {
                Host = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "<unknown>",
                Port = (_tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Port ?? 0,
            };
        }

        private void StartApReadMessagePump()
        {
            _readerCancelToken = new CancellationTokenSource();

            _readerTask = Task.Run(async () =>
            {
                Log.Debug("Starting AP client read pump");

                while (!_readerCancelToken.IsCancellationRequested)
                {
                    try
                    {
                        await ReadResponseAndSetResult(_httpStream, _soapMessageExchange, _readCompletionMap, _readerCancelToken.Token);
                    }
                    catch (IOException e)
                    {
                        /*
                         * This could be useful but we might actually se a "first-fail" scenario, that fixes itself somehow
                         */
                        Log.Information(e, "Stream closed");

                        _readerCancelToken.Cancel();
                        DisposeTcpClient();

                        //Loop through all waiting tasks and make them pay! (or at least inform them that we are out of business for the socket)
                        foreach (var taskCompletionSource in _readCompletionMap)
                        {
                            taskCompletionSource.Value.TrySetException(e);
                        }

                        _readCompletionMap.Clear();
                    }
                    catch (Exception e)
                    {
                        //Keep going i guess!!
                        Log.Warning(e, "Error in read pump");
                    }
                }
                Log.Debug("Ending AP client read pump");
            }, _readerCancelToken.Token);
        }

        public static async Task<bool> ReadResponseAndSetResult(IHttpStream httpStream, IMessageExchange soapMessageExchange, IDictionary<string, TaskCompletionSource<IReplyItem>> readCompletionMap, CancellationToken cancellationToken = default)
        {
            Log.Debug("Reading soap message");
            var item = await soapMessageExchange.ReadResponseAsync<IReplyItem>(httpStream, cancellationToken);
            if (item == null)
            {
                Log.Debug("Null response from AP stream.  Continuing loop");
                return false;
            }
            Log.Debug("Soap message read with handle {reqId}", item?.GetClientRequestHandle());
            if (!readCompletionMap.ContainsKey(item.GetClientRequestHandle()))
            {
                Log.Warning("Got unwanted response with handle: {reqId}", item.GetClientRequestHandle());
                return false;
            }
            Log.Debug("Found waiting task-completion-source for handle {reqId}", item.GetClientRequestHandle());
            var tcs = readCompletionMap[item.GetClientRequestHandle()];
            readCompletionMap.Remove(item.GetClientRequestHandle());
            tcs.TrySetResult(item);
            Log.Debug("Result set on task-completion-source for handle {reqId}", item.GetClientRequestHandle());
            return true;
        }

        private async Task<T> ReadResponse<T>(string reqId)
        {
            /***
             * SHOULD PROBABLY INCLUDE TIMEOUT!!
             */
            using (LogContext.PushProperty("RemoteEndpoint", _debugInfo))
            {
                var tcs = new TaskCompletionSource<IReplyItem>();

                var fireAndfForget = Task.Delay(ResponseReadTimeout, _readerCancelToken.Token).ContinueWith(task => tcs.TrySetCanceled());

                _readCompletionMap[reqId] = tcs;

                /*
                 * This construction is really bad.
                 *
                 * Make sure to remove the tcs from the map and make sure to not fail while doing it.
                 */
                try
                {
                    await tcs.Task;
                }
                finally
                {
                    try
                    {
                        _readCompletionMap.Remove(reqId);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return (T)tcs.Task.Result;
            }
        }

        private async Task WriteAsync(HttpRequest req)
        {
            using (LogContext.PushProperty("RemoteEndpoint", _debugInfo))
            {
                try
                {
                    await _soapMessageExchange.WriteAsync(_httpStream, req);
                }
                catch (Exception e)
                {
                    throw new IOException("Socket is closed", e);
                }
            }
        }

        ~ApClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //release the reader thread (and have it fail if need be)
                if (_readerTask != null)
                {
                    _readerCancelToken.Cancel();
                    DisposeTcpClient(); // Needed to terminate async operation and before waiting on _readerTask.
                    _readerTask.Wait();
                    _readerCancelToken.Dispose();
                    _readerTask = null;
                }

                // Cleanup
                _readCompletionMap.Clear();

                //Dispose the SoapMessageExchange. This will cause any pending blocked reads to fail
                _soapMessageExchange.Dispose();

                //Clean up the TCP connection and accept any errors in pending read/write
                DisposeTcpClient();
            }
        }

        public void DisposeTcpClient()
        {
            _httpStream?.Dispose();
            _tcpClient?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Execute a login with the supplied credentials and wait for response synced with request id
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> Logon(string username, string password)
        {
            string reqId = MessageIdGenerator.GenerateId();

            using (LogContext.PushProperty("RequestId", reqId))
            {
                Log.Debug("Creating HTTP request");
                var request = CreateRequest("LogonRequest");

                Log.Debug("Creating Logon SOAP message with username: {username}", username);
                var logon = ApSoap.CreateLogonRequest(reqId, username, password);

                var enc = Encoding.ASCII;
                Log.Debug("Serializing using encoding: {encoding}", enc.EncodingName);
                request.Content = enc.GetString(SoapXmlSerialize.Serialize(logon));

                request.EnsureDefaultHeaders();

                Log.Debug("Writing HTTP request");
                await WriteAsync(request);

                LogonReply reply;
                try
                {
                    reply = await ReadResponse<LogonReply>(reqId);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "AP logon failed {Host}", _debugInfo);
                    return false;
                }

                if (reply.Error != null)
                {
                    Log.Warning("Got OPC error: {@opcError}", reply.Error);
                    throw ToException(reply.Error);
                }

                if (reply.ClientRequestHandle != reqId)
                {
                    Log.Warning("Remote returned wrong requestId: {WrongRequestId} (Should be {RequestId}",
                        reply.ClientRequestHandle, reqId);
                    return false;
                }

                Log.Debug("Login success");
                return true;
            }
        }

        private Exception ToException(OPCError e)
        {
            return new InvalidOperationException($"[{e.Code}] {e.Text} ({e.Id})");
        }

        /// <summary>
        /// Read a list of tags with a blocking async read. Response matching the req id will be awaited until timeout (or dispose)
        ///
        /// WARNING!
        /// It is very likely that this will fail if just one of the items are not known by the turbine
        ///
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ApValue>> ReadAsync(string[] tags)
        {
            string reqId = MessageIdGenerator.GenerateId();

            using (LogContext.PushProperty("RequestId", reqId))
            {
                Log.Debug("Creating HTTP request");
                var request = CreateRequest("ReadRequest");

                Log.Debug("Creating Read SOAP message with tags: {@tags}", tags);
                var req = ApSoap.CreateReadRequest(reqId, tags);

                var enc = Encoding.ASCII;
                Log.Debug("Serializing using encoding: {encoding}", enc.EncodingName);
                request.Content = enc.GetString(SoapXmlSerialize.Serialize(req));

                request.EnsureDefaultHeaders();

                Log.Debug("Writing HTTP request");
                await WriteAsync(request);
                ReadReply reply;
                try
                {
                    reply = await ReadResponse<ReadReply>(reqId);
                }
                catch (TaskCanceledException)
                {
                    //Timeout or dispose
                    throw new InvalidOperationException($"Response read timed out timeout={ResponseReadTimeout}");
                }

                if (reply?.ReplyBase?.ClientRequestHandle != reqId)
                {
                    Log.Warning("Remote returned wrong requestId: {requestId}", reply?.ReplyBase?.ClientRequestHandle);
                    throw new InvalidOperationException("Message request id mismatch");
                }

                if (PatchItemNamesOnReadReply)
                {
                    /*
                     * This is to be able to handle to result from the park simulator.
                     *
                     * It might be better to just copy the ItemName into ItemPath as that seams to be mixed from the sim.
                     */

                    Log.Debug("Patching item paths. Be careful, we are overwriting information from the source.");
                    int i = 0;
                    foreach (var itemValue in reply.ItemList.SelectMany(r => r.Item.Select(rr => rr)).ToList())
                    {
                        i++;
                        if (string.IsNullOrWhiteSpace(itemValue.ItemPath))
                            itemValue.ItemPath = itemValue.ItemName;
                    }
                }

                return reply.ItemList.SelectMany(r => r.Item.Select(rr => ConvertToApValue(rr, reply.Errors))).ToList();
            }
        }

        public static ApValue ConvertToApValue(ItemValue itemValue, OPCError[] errors)
        {
            var value = itemValue.Item as Value;
            if (value == null)
            {
                var valueError = itemValue.Item as ItemValueError;
                if (valueError == null)
                {
                    throw new ArgumentException($"Item value is unknown. Item: {itemValue.ItemName}.");
                }
                var replyError = errors.FirstOrDefault(e => e.Id == valueError.href);
                var error = replyError == null ? new ApError(-1, "Error reference not found in reply") : new ApError(replyError.Code, replyError.Text);
                return new ApValue(itemValue.ItemPath, error);
            }
            try
            {
                Type type;
                var actualValue = ApSoap.ToObject(value.Val, value.Type, out type);
                return new ApValue(itemValue.ItemPath, actualValue, value.Time, type, value.Quality);
            }
            catch (Exception e)
            {
                return new ApValue(itemValue.ItemPath, new ApError(-1, $"AP value conversion error. {e.Message}. Value: {value.Val}. Type: {value.Type}."));
            }
        }

        public TimeSpan ResponseReadTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public async Task<ApValue> ReadFromTimeAsync(string tag, DateTime fromTime, int maxRows)
        {
            string reqId = MessageIdGenerator.GenerateId();

            using (LogContext.PushProperty("RequestId", reqId))
            {
                Log.Debug("Creating HTTP request");
                var request = CreateRequest("ReadRequest");

                Log.Debug("Creating Logon SOAP message with tag: {tag}", tag);
                var req = ApSoap.CreateReadRequest(reqId, new string[] { tag });

                //WORKS!!
                req.ItemRequestSpec.FromTime = fromTime;
                req.ItemRequestSpec.FromTimeSpecified = true;


                req.ItemRequestSpec.MaxSpan = (ulong)maxRows;
                req.ItemRequestSpec.MaxSpanSpecified = maxRows > 0;

                var enc = Encoding.ASCII;
                Log.Debug("Serializing using encoding: {encoding}", enc.EncodingName);
                request.Content = enc.GetString(SoapXmlSerialize.Serialize(req));

                request.EnsureDefaultHeaders();

                Log.Debug("Writing HTTP request");
                await WriteAsync(request);

                var reply = await ReadResponse<ReadReply>(reqId);


                if (reply?.ReplyBase?.ClientRequestHandle != reqId)
                {
                    Log.Warning("Remote returned wrong requestId: {requestId}", reply?.ReplyBase?.ClientRequestHandle);
                    throw new InvalidOperationException("Message request id mismatch");
                }

                ItemValue iv = reply.ItemList.First().Item.First();

                //Insane deserialize!
                return ConvertToApValue(iv, reply.Errors);
            }
        }

        public async Task<string[]> Browse()
        {
            string reqId = MessageIdGenerator.GenerateId();

            using (LogContext.PushProperty("RequestId", reqId))
            {
                Log.Debug("Creating HTTP request");
                var request = CreateRequest("BrowseRequest");

                Log.Debug("Creating Browse SOAP message");
                var req = ApSoap.CreateBrowseRequest(reqId);

                var enc = Encoding.ASCII;
                Log.Debug("Serializing using encoding: {encoding}", enc.EncodingName);
                request.Content = enc.GetString(SoapXmlSerialize.Serialize(req));

                request.EnsureDefaultHeaders();

                Log.Debug("Writing HTTP request");
                await WriteAsync(request);

                var reply = await ReadResponse<BrowseReply>(reqId);

                if (reply.Error != null)
                {
                    Log.Warning("Got OPC error: {@opcError}", reply.Error);

                    return null;
                }

                if (reply?.ReplyBase?.ClientRequestHandle != reqId)
                {
                    Log.Warning("Remote returned wrong requestId: {requestId}", reply?.ReplyBase?.ClientRequestHandle);
                    return null;
                }

                return reply.BrowseResult.Leaf.Select(s => $"{s.ItemName}, {s.Properties?.Property?.FirstOrDefault()?.Name} = {s.Properties?.Property?.FirstOrDefault()?.Value}").ToArray();
            }
        }

        private HttpRequest CreateRequest(string soapAction)
        {
            var request = new HttpRequest("POST * HTTP/1.1");
            request.AddHeader("Content-Type", "text/xml; charset=ISO-8859-1");
            request.AddHeader("Connection", "Keep-Alive");
            request.AddHeader("SOAPAction", $"http://www.vestas.dk/2001/04/ap#{soapAction}");

            return request;
        }
    }

    class GuidMessageIdGenerator : IMessageIdGenerator
    {
        public string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
