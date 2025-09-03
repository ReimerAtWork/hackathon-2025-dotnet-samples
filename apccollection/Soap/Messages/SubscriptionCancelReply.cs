using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    public class SubscriptionCancelReply
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public OPCError Error { get; set; }
        [XmlAttribute()]
        public string ClientRequestHandle { get; set; }
    }
}