using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class SubscriptionCallbackItemList
    {
        [XmlElement("Item", Form = XmlSchemaForm.Unqualified)]
        public ItemValue[] Item { get; set; }
        [XmlAttribute()]
        public string ItemListHandle { get; set; }
    }
}