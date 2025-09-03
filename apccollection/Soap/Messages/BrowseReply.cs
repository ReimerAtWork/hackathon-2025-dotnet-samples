using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    [XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    public class BrowseReply : IReplyItem
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ReplyBase ReplyBase { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ContinuationPoint { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public OPCError Error { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public BrowseReplyBrowseResult BrowseResult { get; set; }

        public string GetClientRequestHandle()
        {
            return ReplyBase.ClientRequestHandle;
        }
    }
}