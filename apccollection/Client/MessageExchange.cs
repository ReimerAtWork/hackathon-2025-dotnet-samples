using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ApClient.Http;
using ApClient.Soap;
using ApClient.Soap.Messages;
using Serilog;
using HttpRequest = ApClient.Http.HttpRequest;

namespace ApClient.Client
{
    public class MessageExchange : IMessageExchange
    {
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1); //used to write only one message at the time

        public MessageExchange()
        {
        }

        public async Task WriteAsync(IHttpStream httpStream, HttpRequest req, CancellationToken cancellationToken = default)
        {
            if (httpStream == null)
            {
                throw new IOException("Connection closed");
            }

            //Try to get write lock
            try
            {
                Log.Debug("Trying to obtain write-lock with timeout set to {timeout}", WriteLockTimeout);
                await _writeLock.WaitAsync(WriteLockTimeout).ConfigureAwait(false);
                Log.Debug("Got write-lock");
            }
            catch (Exception e)
            {
                //Raise exception, because that's all we can do really.
                throw new InvalidOperationException("Could not obtain write lock", e);
            }

            try
            {
                Log.Debug("Attempting to write http request");
                await httpStream.WriteAsync(req);
                Log.Debug("Http request written");
            }            
            finally
            {
                _writeLock.Release();
                Log.Debug("Released write-lock");
            }
        }

        public async Task<T> ReadResponseAsync<T>(IHttpStream httpStream, CancellationToken cancellationToken = default) where T : IReplyItem
        {
            //!!This might be really bad
            if (httpStream == null)
            {
                //if 10 secs passed and we haven't gotten anything, then just return null. Not a great solution
                return default(T);
            }

            var req = await httpStream.ReadResponseAsync();

            if (req?.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Not OK response code: {req?.StatusCode}");
            }

            Log.Debug("Got a message on client link");

            var xmlReq =
                SoapXmlSerialize.Deserialize(req.Content);

            Log.Debug("Got message of type: {type}", xmlReq?.GetType());

            var item = (xmlReq as Body).Item;

            if (item is T variable)
            {
                return variable;
            }
            else
            {
                throw new InvalidOperationException($"Response is wrong type: {typeof(T)}");
            }
        }

        /// <summary>
        /// Amount of millis to wait for a slot to write a telegram
        /// </summary>
        public int WriteLockTimeout { get; set; } = 5000;

        ~MessageExchange()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writeLock?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}