using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class SubscriptionCallbackReplyBase : ReplyBase
    {
        [XmlAttribute()]
        public System.UInt64 SerialNumber { get; set; }
        [XmlAttribute()]
        public bool Refresh { get; set; }
    }
}