using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class SubscriptionRequestRequestBase : RequestBase
    {
        [XmlAttribute(DataType = "anyURI")]
        public string CallbackURL { get; set; }
        [XmlAttribute(DataType = "duration")]
        public string CallbackRate { get; set; }
        [XmlAttribute()]
        public int SubscriptionPingRate { get; set; }
        [XmlIgnore()]
        public bool SubscriptionPingRateSpecified { get; set; }
    }
}