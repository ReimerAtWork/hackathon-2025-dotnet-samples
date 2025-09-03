using System.IO;
using System.Text;
using System.Xml;

namespace ApClient.Soap
{
    class ApXmlTextWriter : XmlTextWriter
    {
        public ApXmlTextWriter(Stream w, Encoding encoding)
            : base(w, encoding)
        {
        }

        public override void WriteStartDocument()
        {
        }

        public override void WriteStartDocument(bool standalone)
        {
        }        

        public override void WriteString(string text)
        {
            base.WriteRaw(text);
        }
    }
}