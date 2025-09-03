using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ApClient
{
    public class TenMinParser
    {

        /**
         * THIS IS INSANE - PLEASE DO NOT TRUST!!!s
         */
        public TenMinData Parse(string data)
        {
            data = @"<doc xmlns:xsi=""/"">" + data + "</doc>";
            var doc = new XmlDocument();
            doc.LoadXml(data);

            var headers = doc.SelectNodes("//Header").Cast<XmlNode>().Select(s => new {ItemId = s.Attributes["ItemId"].Value, Type = s.Attributes["xsi:type"].Value})
                .ToArray();

            var allRows = doc.SelectSingleNode("//Data").InnerText;

            var lines = allRows.Split(new string[]{"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            var tenMinData = new TenMinData();

            foreach (var line in lines)
            {
                var cols = line.Split(';');

                if (cols.Length != headers.Length)
                    throw new TenMinParserException("Crazy column mismatch - who would have guessed!");

                var row = new TenMinData.Row();
                row.DateTime = DateTime.ParseExact(cols[0], "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime();
                for (int i = 0; i < cols.Length; i++)
                {
                    var colValue = cols[i];
                    var itemId = int.Parse(headers[i].ItemId);
                    var type = headers[i].Type;

                    var val = new TenMinData.KeyValue(itemId, type, colValue);
                    row.Add(val);
                }

                tenMinData.Add(row);
            }

            return tenMinData;
        }
    }

    public class TenMinParserException : Exception
    {
        public TenMinParserException(string message) : base(message)
        {
        }
    }

    public class TenMinData
    {

        public IList<Row> Rows { get; set; } = new List<Row>();

        public class Row
        {
            
            public DateTime DateTime { get; set; }

            public IList<KeyValue> Values { get; set; } = new List<KeyValue>();


            public void Add(KeyValue val)
            {
                Values.Add(val);
            }
        }

        public class KeyValue
        {
  

            public KeyValue(int itemId, string type, string colValue)
            {
                ItemId = itemId;
                Type = type;
                Value = colValue;
            }

            public string Type { get; set; }

            public int  ItemId { get; set; }

            public string  Value { get; set; }
        }

        public void Add(Row row)
        {
            Rows.Add(row);
        }
    }
}