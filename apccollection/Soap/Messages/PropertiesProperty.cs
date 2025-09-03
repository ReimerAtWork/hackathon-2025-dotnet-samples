using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class PropertiesProperty
    {
        [XmlAttribute("type", Namespace = "http://www.w3.org/2001/XMLSchema-instance/")]
        public string Type { get; set; }
        [XmlAttribute()]
        public string Name { get; set; }
        [XmlText()]
        public string Value { get; set; }
    }
}