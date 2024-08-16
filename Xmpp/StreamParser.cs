using EmuWarface.Core;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EmuWarface.Xmpp
{
    internal class StreamParser : IDisposable
    {
        private XmlReader reader;

        private Stream stream;

        public StreamParser(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this.stream = stream;
            reader = XmlReader.Create(stream, new XmlReaderSettings()
            {
                IgnoreProcessingInstructions = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Ignore
            });

            //ReadRootElement();
        }

        public XmlElement NextElement(params string[] expected)
        {
            //byte[] buffer = new byte[12];
            //stream.Read(buffer, 0, 12);

            reader.Read();
            if (reader.NodeType == XmlNodeType.EndElement && reader.Name ==
                "stream:stream")
                throw new IOException("XML stream closed.");
            if (reader.NodeType != XmlNodeType.Element)
                throw new XmlException("Unexpected node: '" + reader.Name +
                    "' of type " + reader.NodeType);
            if (!reader.IsStartElement())
                throw new XmlException("Not a start element: " + reader.Name);

            using (XmlReader inner = reader.ReadSubtree())
            {
                inner.Read();
                string xml = inner.ReadOuterXml();
                XmlDocument doc = new XmlDocument();
                using (var sr = new StringReader(xml))
                using (var xtr = new XmlTextReader(sr))
                    doc.Load(xtr);
                XmlElement elem = (XmlElement)doc.FirstChild;
                if (elem.Name == "stream:error")
                {
                    string condition = elem.FirstChild != null ?
                        elem.FirstChild.Name : "undefined";

                    throw new ServerException("Unrecoverable stream error: " + condition);
                }
                if (expected.Length > 0 && !expected.Contains(elem.Name))
                    throw new XmlException("Unexpected XML element: " + elem.Name);

                return elem;
            }
        }

        public void Dispose()
        {
            reader.Close();
            stream.Close();
        }

        public void ReadRootElement()
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.XmlDeclaration:
                        break;

                    case XmlNodeType.Element:
                        if (reader.Name == "stream:stream")
                        {
                            string lang = reader.GetAttribute("xml:lang");
                            if (!string.IsNullOrEmpty(lang))
                                return;
                        }
                        throw new XmlException("Unexpected document root: " + reader.Name);
                    default:
                        throw new XmlException("Unexpected node: " + reader.Name);
                }
            }
        }
    }
}