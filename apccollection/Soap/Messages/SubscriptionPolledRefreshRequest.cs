using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    public class SubscriptionPolledRefreshRequest
    {
        [XmlAttribute()]
        public string ClientRequestHandle { get; set; }
        [XmlAttribute()]
        public string ServerSubHandle { get; set; }
        [XmlAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReturnAllItems { get; set; } = false;
    }
}