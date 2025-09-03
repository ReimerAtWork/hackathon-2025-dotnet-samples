using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class ReadRequestRequestBase : RequestBase
    {
        [XmlAttribute()]
        public long ScanTimeHint { get; set; } // msec. hint when the client will aks for the same data again
        [XmlIgnore()]
        public bool ScanTimeHintSpecified { get; set; }
    }
}