using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class OPCError
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public int Code { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Text { get; set; }

        [XmlAttribute(DataType = "ID")]
        public string Id { get; set; }
    }
}