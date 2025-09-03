using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    public class SubscriptionCancelRequest
    {
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string ClientRequestHandle { get; set; }
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string ServerSubHandle { get; set; }
    }
}