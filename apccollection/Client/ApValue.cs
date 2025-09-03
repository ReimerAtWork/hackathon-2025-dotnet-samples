using System;

namespace ApClient.Client
{
    public partial class ApClient
    {
        public class ApError
        {
            public ApError(int code, string text)
            {
                Code = code;
                Text = text;
            }

            public override string ToString()
            {
                return $"{Text} ({Code})";
            }

            public int Code { get; }

            public string Text { get; }
        }

        public class ApValue
        {
            public ApValue(string tag, ApError error)
            {
                Tag = tag;
                Error = error;
            }

            public ApValue(string tag, object value, DateTime time, Type type, byte quality)
            {
                Tag = tag;
                ValueType = type;
                Value = value;
                Time = time;
                Quality = quality;
            }

            public Type ValueType { get; }

            public object Value { get;}

            public DateTime Time { get; }

            public string Tag { get; }

            public byte Quality { get; }

            public ApError Error { get; }
        }
    }
}