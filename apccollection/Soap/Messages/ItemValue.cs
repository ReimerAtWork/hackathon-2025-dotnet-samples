using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class ItemValue
    {
        [XmlElement("Value", typeof(Value), Form = XmlSchemaForm.Unqualified)]
        [XmlElement("Values", typeof(ItemValueValues), Form = XmlSchemaForm.Unqualified)]
        [XmlElement("Properties", typeof(Properties), Form = XmlSchemaForm.Unqualified)]
        [XmlElement("Error", typeof(ItemValueError), Form = XmlSchemaForm.Unqualified)]
        public object Item { get; set; }
        [XmlAttribute()]
        public string ItemPath { get; set; } // A portion of the namespace pointing to the data.
        [XmlAttribute()]
        public string ItemName { get; set; } // Identifier of the Data.
        [XmlAttribute()]
        public string ClientItemHandle { get; set; } // Client specific value
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