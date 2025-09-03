using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class BrowseRequestStartPoint
    {
        [XmlAttribute()]
        public string ItemPath { get; set; }
        [XmlText()]
        public string Value { get; set; }
    }
}