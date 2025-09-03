using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlInclude(typeof(ReadRequestRequestBase))]
    public class RequestBase
    {
        [XmlAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReturnItemTime { get; set; } = false; // Indicates whether to return item time for each item 
        [XmlAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReturnItemName { get; set; } = false; // Indicates whether to return item ID for each item
        [XmlAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReturnItemPath { get; set; } = false; // Indicates whether to return ItemPath for each item
        [XmlAttribute(DataType = "duration")]
        public string RequestTimeout { get; set; } // Indicates the maximum time that the client wants to wait for the Server to process a reply
        [XmlAttribute()]
        public string ClientRequestHandle { get; set; } // client specific value
    }
}