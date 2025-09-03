using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [XmlRoot(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public class Fault
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public XmlQualifiedName faultcode { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string faultstring { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string faultactor { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public detail detail { get; set; }
    }
}