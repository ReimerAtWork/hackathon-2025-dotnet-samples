using System;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class ItemRequestSpec
    {
        [XmlAttribute()]
        public string ItemPath { get; set; } // A portion of the namespace pointing to the data. ItemPath will not imply node, server, or group.
        [XmlAttribute(DataType = "duration")]
        public string MaxAge { get; set; } // Indicates the requested “freshness” of the data.
        [XmlAttribute()]
        public System.Double DeadBand { get; set; } // Specifies the percentage of full engineering unit range of an item’s value must change prior to being sent back in a Callback.
        [XmlIgnore()]
        public bool DeadBandSpecified { get; set; }
        [XmlAttribute()]
        public string ReqType { get; set; } // Specifies the client’s requested type for the Item’s value to be returned by the server.
        [XmlAttribute()]
        public DateTime FromTime { get; set; } // A timestamp used only with historical items to specify the beginning of a collection.
        [XmlIgnore()]
        public bool FromTimeSpecified { get; set; }
        [XmlAttribute()]
        public DateTime ToTime { get; set; } // A timestamp used only with historical items to specify the end of a collection.
        [XmlIgnore()]
        public bool ToTimeSpecified { get; set; }
        [XmlAttribute()]
        public ulong FromIndex { get; set; } // A unsigned long used only with historical items to specify the beginning of a collection.
        [XmlIgnore()]
        public bool FromIndexSpecified { get; set; }
        [XmlAttribute()]
        public ulong ToIndex { get; set; } // A unsigned long used only with historical items to specify the end of a collection.
        [XmlIgnore()]
        public bool ToIndexSpecified { get; set; }
        [XmlAttribute()]
        public ulong MaxSpan { get; set; } // Specifies the maximum number of returned historical item values.
        [XmlIgnore()]
        public bool MaxSpanSpecified { get; set; }
    }
}