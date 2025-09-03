using ApClient.Soap.Messages;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ApClient.Soap
{
    public static class SoapXmlSerialize
    {
        private static XmlSerializer _soapSerializer = new XmlSerializer(typeof(Envelope));
        private static XmlSerializerNamespaces _soapNs = new XmlSerializerNamespaces();

        static SoapXmlSerialize()
        {
            _soapSerializer.UnknownElement += (sender, args) =>
                Console.WriteLine($"UE {args.ExpectedElements}");
            _soapSerializer.UnknownAttribute += (sender, args) => Console.WriteLine($"UA {args.Attr}");
            _soapSerializer.UnknownNode += (sender, args) => Console.WriteLine($"UN {args.Name}");
            _soapSerializer.UnreferencedObject += (sender, args) =>
                Console.WriteLine($"UO {args.UnreferencedObject}");

            _soapNs.Add("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");
            _soapNs.Add("AP-MSG", "http://www.vestas.dk/2001/04/ap");
        }

        public static object Deserialize(string content)
        {
            using (var stringReader = new StringReader(content))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                var envelope = (Envelope)_soapSerializer.Deserialize(xmlReader);
                return envelope.Body;
            }
        }

        public static byte[] Serialize(object reply)
        {
            Envelope envelope = new Envelope { Body = new Body { Item = reply } };

            using (var mem = new MemoryStream())
            using (var writer = new ApXmlTextWriter(mem, Encoding.ASCII)
            {
                Formatting = Formatting.None,
            })
            {
                _soapSerializer.Serialize(writer, envelope, _soapNs);


                //Special handling of this crazy break of comliancy by the wtg software.
                if (reply is BrowseRequest)
                {
                    return InsaneXmlNamespaceCompliancyFixForBrowseRequest(mem.ToArray());
                }

                return mem.ToArray();
            }
        }

        private static byte[] InsaneXmlNamespaceCompliancyFixForBrowseRequest(byte[] data)
        {
            using (var mem = new MemoryStream(data))
            using (var mem2 = new MemoryStream())
            {
                var doc = new XmlDocument();
                doc.Load(new MemoryStream(data));
                var elm = doc.GetElementsByTagName("AP-MSG:BrowseRequest")[0];

                var attr = doc.CreateAttribute("xmlns:AP-MSG");
                attr.Value = "http://www.vestas.dk/2001/04/ap";
                elm.Attributes.Append(attr);

                doc.Save(mem2);
                return mem2.ToArray();
            }
        }
    }
}