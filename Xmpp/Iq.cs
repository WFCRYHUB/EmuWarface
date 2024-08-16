using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Xmpp
{
    public class Iq : Stanza
    {
        public Iq(IqType type, Jid to = null, Jid from = null,
            XmlElement data = null)
            : base(null, to, from, data)
        {
            Type = type;
        }

        public Iq(XmlElement element)
            : base(element)
        {
        }

        private static ulong _id = 0;
        public static string GenerateId()
        {
            _id++;
            return "uid" + _id.ToString("x8");
        }
        private IqType ParseType(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var dict = new Dictionary<string, IqType>() {
                { "set", IqType.Set },
                { "get", IqType.Get },
                { "result", IqType.Result },
                { "error", IqType.Error }
            };
            return dict[value];
        }

        public IqType Type
        {
            get
            {
                return ParseType(Element.GetAttribute("type"));
            }

            set
            {
                var v = value.ToString().ToLowerInvariant();
                Element.SetAttribute("type", v);
            }
        }

        public bool IsRequest
        {
            get
            {
                var t = Type;
                return t == IqType.Set || t == IqType.Get;
            }
        }

        public bool IsResponse
        {
            get
            {
                return !IsRequest;
            }
        }

        public void SwapDirection()
        {
            Jid temp = From;
            From = To;
            To = temp;
        }

        public void Compress()
        {
            if (Query == null)
                return;

            string text = Query.OuterXml;

            XmlElement data = Xml.Element("data");

            foreach (XmlAttribute attr in Query.Attributes)
            {
                data.Attr(attr.Name, attr.Value);
            }

            data.Attr("query_name", Query.Name)
                .Attr("compressedData", Convert.ToBase64String(ZlibStream.CompressBuffer(Encoding.UTF8.GetBytes(text))))
                .Attr("originalSize", Encoding.UTF8.GetByteCount(text));
                //.Attr("originalSize", text.Length);

            Element["query"].RemoveAll();
            Element["query"].Child(data);
        }

        public void Uncompress()
        {
            if (Query == null)
                return;

            string data = Query.GetAttribute("compressedData");

            if (!string.IsNullOrEmpty(data))
            {
                Element["query"].RemoveAll();
                Element["query"].Child(Xml.Parse(Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(Convert.FromBase64String(data)))));
            }
        }

        public Iq SetError(object custom_code) => SetError(Convert.ToInt32(custom_code));
        public Iq SetError(int custom_code)
        {
            if (Query != null)
            {
                XmlElement error = Xml.Element("error").Attr("type", "continue").Attr("code", "8").Attr("custom_code", custom_code);
                error.Child(Xml.Element("internal-server-error", "urn:ietf:params:xml:ns:xmpp-stanzas"));
                error.Child(Xml.Element("text", "urn:ietf:params:xml:ns:xmpp-stanzas").Text("Custom query error"));

                Element.Child(error);
            }

            return this;
        }

        public Iq SetQuery(XmlElement node)
        {
            if (Element["query"] == null)
                Element.Child(Xml.Element("query", "urn:cryonline:k01"));

            Element["query"].RemoveAll();
            Element["query"].Child(node);

            return this;
        }
    }
}
