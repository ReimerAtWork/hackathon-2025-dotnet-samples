using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    public class ReadRequest
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ReadRequestRequestBase RequestBase { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ItemRequestSpec ItemRequestSpec { get; set; }
        [XmlElement("ItemList", Form = XmlSchemaForm.Unqualified)]
        public ReadRequestItemList[] ItemList { get; set; }
    }
}