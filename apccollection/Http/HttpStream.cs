using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace ApClient.Http
{
    public class HttpStream : IHttpStream, IDisposable
    {
        private readonly Stream _stream;
        private readonly StreamReader _streamReader;
        private readonly StreamWriter _streamWriter;

        public HttpStream(Stream stream)
        {
            _stream = stream;
            _streamReader = new StreamReader(_stream, Encoding.ASCII);
            _streamWriter = new StreamWriter(_stream, Encoding.ASCII);

            /*
             * The most important line of code in this file.
             *
             * ParkSim (and potentially also wtg software) require this particular newline setting when reading.
             *
             * StreamReader has built in automatic newline detection, so no reason to worry about that.
             */
            _streamWriter.NewLine = "\r\n";
        }

        public async Task<HttpRequest> ReadAsync()
        {
            HttpRequest request = null;

            while (true)
            {
                try
                {
                    var line = await _streamReader.ReadLineAsync();

                    if (line == "POST * HTTP/1.1")
                    {
                        request = new HttpRequest(line);
                    }
                    else if (request != null)
                    {
                        if (line == "")
                        {
                            //We are done here!
                            break;
                        }
                        else
                        {
                            request.AddHeader(line);
                        }
                    }
                }
                catch (Exception e)
                {
                    var state = new
                    {
                        StreamReaderNull = _streamReader == null,
                        StreamWriterNull = _streamWriter == null,
                        StreamNull = _stream == null,
                        StreamType = _stream?.GetType()
                    };
                    Log.Warning(e, "Error in read, giving it a rest. State={@state}", state);
                    throw;
                }
            }

            char[] buf = new char[request.ContentLength()];
            await _streamReader.ReadAsync(buf, 0, buf.Length);
            request.Content = new string(buf);

            return request;
        }

        public async Task<HttpResp> ReadResponseAsync()
        {
            HttpResp request = null;

            while (true)
            {
                try
                {
                    var line = await _streamReader.ReadLineAsync();

                    Log.Debug("Received a line: {line}", line);

                    /*
                     * It looks like this can happen on Linux
                     *
                     * Maybe this is when the socket has closed and we need to raise an IOExxception to inform the message exchange that the, that the fun is over.
                     */
                    if (line == null)
                    {
                        throw new IOException("Blank line received, indicates socket is dead.");
                    }

                    if (line.StartsWith("HTTP/1.1"))
                    {
                        Log.Debug("Line identified as response start: {line}", line);
                        request = new HttpResp(line);
                    }
                    else if (request != null)
                    {
                        if (line == "" || line == " ") //WTG SENDS SPACE!!!!
                        {
                            Log.Debug("Received a terminator line: '{line}'", line);
                            //We are done here!
                            break;
                        }
                        else
                        {
                            Log.Debug("Received a header: {line}", line);
                            request.AddHeader(line);
                        }
                    }
                }
                catch (Exception e)
                {
                    var state = new
                    {
                        StreamReaderNull = _streamReader == null,
                        StreamWriterNull = _streamWriter == null,
                        StreamNull = _stream == null,
                        StreamType = _stream?.GetType()
                    };
                    Log.Warning(e, "Error in read response, giving it a rest. State={@state}", state);
                    throw;
                }
            }

            Log.Debug("Reading content of {bytes} bytes", request.ContentLength());
            if (request.ContentLength() > 0)
            {
                int read = 0;

                char[] buf = new char[request.ContentLength()];
                while (read < buf.Length)
                {
                    read += await _streamReader.ReadAsync(buf, read, buf.Length - read);
                }

                request.Content = new string(buf);
            }
            Log.Debug("Reading content done");

            return request;
        }

        public async Task WriteAsync(HttpResp httpResp)
        {
            httpResp.EnsureDefaultHeaders();

            using (var mem = new MemoryStream())
            using (var w = new StreamWriter(mem, _streamWriter.Encoding))
            {
                w.NewLine = _streamWriter.NewLine;

                await w.WriteLineAsync($"HTTP/1.1 {(int)httpResp.StatusCode} {httpResp.StatusCode}");

                foreach (var h in httpResp.Headers)
                {
                    await w.WriteLineAsync($"{h.Key}:{h.Value}");
                }

                await w.WriteLineAsync();
                if (httpResp.Content != null)
                {
                    await w.WriteAsync(httpResp.Content);
                }

                await w.FlushAsync();
                var str = Encoding.ASCII.GetString(mem.ToArray());
                await _streamWriter.WriteAsync(str);
                await _streamWriter.FlushAsync();
            }
        }

        public async Task WriteAsync(HttpRequest httpRequest)
        {
            httpRequest.EnsureDefaultHeaders();

            using (var mem = new MemoryStream())
            using (var w = new StreamWriter(mem, _streamWriter.Encoding) { NewLine = _streamWriter.NewLine })
            {
                await w.WriteLineAsync(httpRequest.FirstLine);

                foreach (var h in httpRequest.Headers)
                {
                    /*
                     * Avoid using space after colon before header value. ParkSim does not approve.
                     *
                     * OK format = <key>:<header>
                     */
                    await w.WriteLineAsync($"{h.Key}:{h.Value}");
                }

                await w.WriteLineAsync();
                if (httpRequest.Content != null)
                {
                    await w.WriteAsync(httpRequest.Content);
                }

                await w.FlushAsync();
                var str = Encoding.ASCII.GetString(mem.ToArray());
                await _streamWriter.WriteAsync(str);
                await _streamWriter.FlushAsync();
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _streamReader?.Dispose();
            _streamWriter?.Dispose();
        }
    }
}