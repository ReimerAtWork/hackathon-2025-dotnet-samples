using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class ItemValueError
    {
        [XmlAttribute(DataType = "anyURI")]
        public string href { get; set; } // The attribute pointer to the appropriate Error code located elsewhere in the Reply.
        [XmlAttribute(DataType = "ID")]
        public string id { get; set; } // Specifies the ID of the OPCError, which will be referred to by the Individ-ual Item Errors.
    }
}