using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class WriteRequestRequestBase : RequestBase
    {
        [XmlAttribute()]
        public string LogName { get; set; }

        [XmlAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReturnItemVal { get; set; } = false;
    }
}