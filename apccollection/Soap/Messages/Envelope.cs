using System.Xml;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [XmlRoot(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public class Envelope
    {
        public Header Header { get; set; }

        public Body Body { get; set; }

        [XmlAnyElement()]
        public XmlElement[] Any { get; set; }

        [XmlAnyAttribute()]
        public XmlAttribute[] AnyAttr { get; set; }
    }
}