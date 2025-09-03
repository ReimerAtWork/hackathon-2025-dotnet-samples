using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [XmlRoot(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public class Body
    {
        [XmlElement("SubscriptionCallback", typeof(SubscriptionCallback), Namespace ="http://www.vestas.dk/2001/04/ap")]
        [XmlElement("SubscriptionRequest", typeof(SubscriptionRequest), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("SubscriptionReply", typeof(SubscriptionReply), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("SubscriptionCancelRequest", typeof(SubscriptionCancelRequest), Namespace ="http://www.vestas.dk/2001/04/ap")]
        [XmlElement("SubscriptionCancelReply", typeof(SubscriptionCancelReply), Namespace ="http://www.vestas.dk/2001/04/ap")]
        [XmlElement("ReadRequest", typeof(ReadRequest), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("ReadReply", typeof(ReadReply), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("WriteRequest", typeof(WriteRequest), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("WriteReply", typeof(WriteReply), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("LogonRequest", typeof(LogonRequest), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("LogonReply", typeof(LogonReply), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("LogoffRequest", typeof(LogoffRequest), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("LogoffReply", typeof(LogoffReply), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("BrowseRequest", typeof(BrowseRequest), Namespace = "http://www.vestas.dk/2001/04/ap")]
        [XmlElement("BrowseReply", typeof(BrowseReply), Namespace = "http://www.vestas.dk/2001/04/ap")]
        public object Item { get; set; }
    }
}