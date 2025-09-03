using System;
using System.Collections.Generic;
using System.Net;

namespace ApClient.Http
{
    public class HttpResp
    {
        public HttpStatusCode StatusCode { get; }
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public HttpResp(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;            
        }

        public HttpResp(string line)
        {
            var split = line.Replace("HTTP/1.1", "").Trim().Split(' ');
            StatusCode = (HttpStatusCode) int.Parse(split[0]);
        }

        public void AddHeader(string key, object value)
        {
            Headers[key] = Convert.ToString(value);
        }

        public void AddHeader(string key, string value)
        {

            Headers[key] = value;
        }

        public void AddHeader(string line)
        {
            var split = line.Split(':');
            Headers[split[0]] = split[1];
        }

        public string Content { get; set; }

        public void EnsureDefaultHeaders()
        {
            Default("Content-Length", Content?.Length ?? 0);
            Default("Date", DateTime.Now);            
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
        
    }
}