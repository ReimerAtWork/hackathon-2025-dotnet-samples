using System.Xml;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class detail
    {
        [XmlAnyElement()]
        public XmlElement[] Any { get; set; }

        [XmlAnyAttribute()]
        public XmlAttribute[] AnyAttr { get; set; }
    }
}