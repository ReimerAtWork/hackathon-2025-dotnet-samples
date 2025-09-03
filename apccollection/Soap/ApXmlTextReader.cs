using System.IO;
using System.Xml;

namespace ApClient.Soap
{
    class ApXmlTextReader : XmlTextReader
    {
        private readonly string _httpWwwW3OrgXmlschemaInstance = "http://www.w3.org/2001/XMLSchema-instance";
        private static string _strB = "type";

        public ApXmlTextReader(Stream input)
            : base(input)
        {
        }

        public ApXmlTextReader(TextReader input)
            : base(input)
        {
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            if (localName.CompareTo(_strB) == 0 && namespaceURI.CompareTo(_httpWwwW3OrgXmlschemaInstance) == 0)
                return null;
            return base.GetAttribute(localName, namespaceURI);
        }
    }
}