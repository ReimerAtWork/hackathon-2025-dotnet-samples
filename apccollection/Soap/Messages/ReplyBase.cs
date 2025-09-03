using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class ReplyBase
    {
        [XmlAttribute()]
        public System.DateTime RcvTime { get; set; } // The timestamp associated with the server receiving the Request.
        [XmlAttribute()]
        public System.DateTime ReplyTime { get; set; } // The timestamp associated with the server returning the Reply.
        [XmlAttribute()]
        public string ClientRequestHandle { get; set; } // client specific value
    }
}