using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    public class BrowseReplyBrowseResultBranch
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string BranchPath { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string BranchName { get; set; }
    }
}