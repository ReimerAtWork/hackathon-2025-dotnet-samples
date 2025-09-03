using System;
using System.Collections.Generic;

namespace ApClient.Http
{
    public class HttpRequest
    {
        public string FirstLine { get; }

        public HttpRequest(string firstLine)
        {
            FirstLine = firstLine;
        }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public void AddHeader(string line)
        {
            var split = line.Split(':');
            Headers[split[0]] = split[1];
        }

        public void AddHeader(string key, string value)
        {
            
            Headers[key] = value;
        }

        public void EnsureDefaultHeaders()
        {
            Default("Content-Length", Content?.Length ?? 0);
        }

        private void Default(string key, object val)
        {
            if (!Headers.ContainsKey(key))
            {
                Headers[key] = Convert.ToString(val);
            }
        }

        public int ContentLength()
        {
            return int.Parse(Headers["Content-Length"]);
        }

        public string Content { get; set; }
    }
}