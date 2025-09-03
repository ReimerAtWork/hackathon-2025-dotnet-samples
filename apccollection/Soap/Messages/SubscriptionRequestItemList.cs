using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class SubscriptionRequestItemList
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ItemRequestSpec ItemRequestSpec { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public RequestItem[] Item { get; set; }
        [XmlAttribute()]
        public string ItemListHandle { get; set; }
        [XmlAttribute(DataType = "duration")]
        public string CallbackRate { get; set; }
    }
}