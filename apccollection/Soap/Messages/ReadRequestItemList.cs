using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class ReadRequestItemList
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ItemRequestSpec ItemRequestSpec { get; set; }
        [XmlElement("Item", Form = XmlSchemaForm.Unqualified)]
        public RequestItem[] Item { get; set; }
    }
}