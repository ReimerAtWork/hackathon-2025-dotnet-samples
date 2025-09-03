using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class BrowseReplyBrowseResultLeaf
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ItemPath { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ItemName { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public Properties Properties { get; set; }
    }
}