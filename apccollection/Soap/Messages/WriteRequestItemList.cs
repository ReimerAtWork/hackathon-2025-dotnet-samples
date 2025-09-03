using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class WriteRequestItemList
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ItemRequestSpec ItemRequestSpec { get; set; }
        [XmlElement("Item", Form = XmlSchemaForm.Unqualified)]
        public ItemValue[] Item { get; set; }
    }
}