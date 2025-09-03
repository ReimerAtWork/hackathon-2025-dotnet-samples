using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class ItemValueValues
    {
        [XmlElement("Value", Form = XmlSchemaForm.Unqualified)]
        public Value[] Value { get; set; }
        [XmlAttribute()]
        public System.DateTime Time { get; set; } // Optionally returned if requested by the ReturnItemTime Attribute.
        [XmlIgnore()]
        public bool TimeSpecified { get; set; }
        [XmlAttribute()]
        [System.ComponentModel.DefaultValueAttribute(typeof(System.Byte), "3")]
        public System.Byte Quality { get; set; } = ((System.Byte)(3)); // A numeric value matching the OPC Quality Flags: 0-Bad, 1-Uncertain, and 3-Good.
        [XmlAttribute()]
        public System.Byte SubStatus { get; set; } // A “Good” quality will result in no SubStatus attribute being returned. If “Bad”, or “Uncertain” then a SubStatus attribute will be returned.
        [XmlIgnore()]
        public bool SubStatusSpecified { get; set; }
        [XmlAttribute()]
        public System.Byte LimitBits { get; set; } // A “Good” quality will result in no LimitBits attribute being returned. If “Bad”, or “Uncertain” then a Limit attribute will be returned.
        [XmlIgnore()]
        public bool LimitBitsSpecified { get; set; }
        [XmlAttribute()]
        public System.Byte VendorBits { get; set; } // A “Good” quality will result in no VendorBits attribute being returned. If “Bad”, or “Uncertain” then a VendorBits attribute will be returned.
        [XmlIgnore()]
        public bool VendorBitsSpecified { get; set; }
        [XmlAttribute("type", Namespace = "http://www.w3.org/2001/XMLSchema-instance/")]
        public string Type { get; set; } // xsi type - not used
        [XmlAttribute()]
        public string arrayType { get; set; } // e.g. xsi:int[10]
    }
}