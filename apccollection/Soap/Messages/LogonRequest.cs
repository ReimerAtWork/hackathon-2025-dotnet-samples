using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    public class LogonRequest
    {
        [XmlAttribute(Namespace = "http://www.vestas.dk/2001/04/ap")]
        public string UserName { get; set; }
        [XmlAttribute(Namespace = "http://www.vestas.dk/2001/04/ap")]
        public string Password { get; set; }
        [XmlAttribute(Namespace = "http://www.vestas.dk/2001/04/ap")]
        public string ClientRequestHandle { get; set; }
        [XmlAttribute(Namespace = "http://www.vestas.dk/2001/04/ap")]
        public string Language { get; set; }
    }
}