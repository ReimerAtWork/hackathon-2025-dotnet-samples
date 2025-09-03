using System;
using System.Collections.Generic;

namespace ApClient.Console
{
    public class CheckmarkRepository
    {
        private Dictionary<string, DateTime> _checkmarks = new Dictionary<string, DateTime>();

        public DateTime GetCheckmark(string id)
        {
            if(!_checkmarks.ContainsKey(id))
                return DateTime.Now.AddDays(-32);//Go back a month -that is max.
            return _checkmarks[id];
        }

        public void SetCheckmark(string id, DateTime mark)
        {
            _checkmarks[id] = mark;
        }

        public void Save()
        {

        }

        public void Load()
        {

        }
    }

    public class TenMinPollConfig
    {
        public string UnitId { get; set; }

        public string Hostname { get; set; }

        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}