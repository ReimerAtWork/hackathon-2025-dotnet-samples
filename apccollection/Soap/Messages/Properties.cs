using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class Properties
    {
        [XmlElement("Property", Form = XmlSchemaForm.Unqualified)]
        public PropertiesProperty[] Property { get; set; }
        [XmlAttribute(Namespace = "http://schemas.xmlsoap.org/soap/encoding/")]
        public string arrayType { get; set; }
    }
}