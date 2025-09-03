using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class RequestItem
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ItemRequestSpec ItemRequestSpec { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public Properties Properties { get; set; } // requesting property values
        [XmlAttribute()]
        public string ItemName { get; set; } // item name, maybe relative
        [XmlAttribute()]
        public string ClientItemHandle { get; set; } // client specific value
    }
}