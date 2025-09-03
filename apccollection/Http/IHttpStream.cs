using System.Threading.Tasks;

namespace ApClient.Http
{
    public interface IHttpStream
    {
        Task<HttpResp> ReadResponseAsync();
        Task WriteAsync(HttpRequest httpRequest);
    }
}