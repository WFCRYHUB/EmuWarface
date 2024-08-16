using System;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace EmuWarface.Xmpp
{
    public static class Xml
    {
        public static XmlElement Load(string path)
        {
            using (XmlReader reader = XmlReader.Create(path, new XmlReaderSettings() { IgnoreComments = true }))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                return doc.DocumentElement;
            }
        }

        public static XmlElement Parse(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }

        public static XmlElement Element(string name, string @namespace = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            return new XmlDocument().CreateElement(name, @namespace);
        }

        public static XmlElement Child(this XmlElement e, XmlNode child)
        {
            if (child == null)
                return e;

            XmlNode imported = e.OwnerDocument.ImportNode(child, true);
            e.AppendChild(imported);
            return e;
        }

        public static XmlElement Attr(this XmlElement e, string name, object value)
        {
            //if (value == null)
            //    return e;

            e.SetAttribute(name, value == null ? "" : value.ToString());
            return e;
        }

        public static XmlElement Text(this XmlElement e, string text)
        {
            e.AppendChild(e.OwnerDocument.CreateTextNode(text));
            return e;
        }

        public static string ToXmlString(this XmlElement e, bool xmlDeclaration = false,
            bool leaveOpen = false)
        {
            StringBuilder b = new StringBuilder("<" + e.Name);
            if (!string.IsNullOrEmpty(e.NamespaceURI))
                b.Append(" xmlns='" + e.NamespaceURI + "'");
            foreach (XmlAttribute a in e.Attributes)
            {
                if (a.Name == "xmlns")
                    continue;
                if (a.Value != null)
                    b.Append(" " + a.Name + "='" + SecurityElement.Escape(a.Value.ToString())
                        + "'");
            }
            if (e.IsEmpty)
                b.Append("/>");
            else
            {
                b.Append(">");
                foreach (var child in e.ChildNodes)
                {
                    if (child is XmlElement)
                        b.Append(((XmlElement)child).ToXmlString());
                    else if (child is XmlText)
                        b.Append(((XmlText)child).InnerText);
                }
                b.Append("</" + e.Name + ">");
            }
            string xml = b.ToString();

            if (xmlDeclaration)
                xml = "<?xml version='1.0' ?>" + xml;
            if (leaveOpen)
                return Regex.Replace(xml, "/>$", ">");
            return xml;
        }
    }
}