using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class BrowseReplyBrowseResult
    {
        [XmlElement("Branch", Form = XmlSchemaForm.Unqualified)]
        public BrowseReplyBrowseResultBranch[] Branch { get; set; }
        [XmlElement("Leaf", Form = XmlSchemaForm.Unqualified)]
        public BrowseReplyBrowseResultLeaf[] Leaf { get; set; }
    }
}