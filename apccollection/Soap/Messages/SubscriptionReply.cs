using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    public class SubscriptionReply
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public SubscriptionReplyReplyBase ReplyBase { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public SubscriptionReplyItemList[] ItemList { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public OPCError[] Errors { get; set; }
    }
}