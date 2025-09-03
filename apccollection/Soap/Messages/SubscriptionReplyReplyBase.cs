using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class SubscriptionReplyReplyBase : ReplyBase
    {
        [XmlAttribute()]
        public string ServerSubHandle { get; set; }
        [XmlAttribute(DataType = "duration")]
        public string ActualCallbackRate { get; set; }
    }
}