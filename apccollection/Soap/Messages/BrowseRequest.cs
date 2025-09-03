using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApClient.Soap.Messages
{
    //[XmlType(Namespace = "http://www.vestas.dk/2001/04/ap")]
    [XmlRoot(Namespace = "http://www.vestas.dk/2001/04/ap", IsNullable = false)]
    
    public class BrowseRequest
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public RequestBase RequestBase { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public BrowseRequestStartPoint StartPoint { get; set; }
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ContinuationPoint { get; set; }

        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ReturnBranches { get; set; } = true;

        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReturnProperties { get; set; } = false;
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public System.UInt64 MaxItemsReturned { get; set; }
        [XmlIgnore()]
        public bool MaxItemsReturnedSpecified { get; set; }
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string BranchFilter { get; set; }
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string LeafFilter { get; set; }
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string VendorFilter { get; set; }
    }
}