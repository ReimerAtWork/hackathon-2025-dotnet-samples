using System;
using System.Threading;
using System.Threading.Tasks;
using ApClient.Http;
using ApClient.Soap.Messages;
using HttpRequest = ApClient.Http.HttpRequest;

namespace ApClient.Client
{
    public interface IMessageExchange : IDisposable
    {
        Task WriteAsync(IHttpStream httpStream, HttpRequest req, CancellationToken cancellationToken = default);
        Task<T> ReadResponseAsync<T>(IHttpStream httpStream, CancellationToken cancellationToken = default) where T : IReplyItem;

        /// <summary>
        /// Amount of millis to wait for a slot to write a telegram
        /// </summary>
        int WriteLockTimeout { get; set; }
    }
}