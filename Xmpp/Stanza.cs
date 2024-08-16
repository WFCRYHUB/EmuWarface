using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Xmpp
{
    public abstract class Stanza
    {
        public XmlElement Element { get; private set; }

        public Jid To
        {
            get
            {
                string v = Element.GetAttribute("to");
                return string.IsNullOrEmpty(v) ? null : new Jid(v);
            }

            set
            {
                if (value == null)
                    Element.RemoveAttribute("to");
                else
                    Element.SetAttribute("to", value.ToString());
            }
        }

        public Jid From
        {
            get
            {
                string v = Element.GetAttribute("from");
                return string.IsNullOrEmpty(v) ? null : new Jid(v);
            }

            set
            {
                if (value == null)
                    Element.RemoveAttribute("from");
                else
                    Element.SetAttribute("from", value.ToString());
            }
        }

        public string Id
        {
            get
            {
                var v = Element.GetAttribute("id");
                return string.IsNullOrEmpty(v) ? null : v;
            }

            set
            {
                if (value == null)
                    Element.RemoveAttribute("id");
                else
                    Element.SetAttribute("id", value);
            }
        }

        public XmlElement Query
        {
            get
            {
                //var query = (XmlElement)Element["query"]?.FirstChild;

                //if (query == null)
                //    throw new InvalidOperationException("Query does not exist");

                return (XmlElement)Element["query"]?.FirstChild;
            }
        }

        public Stanza(string @namespace = null, Jid to = null, Jid from = null, params XmlElement[] data)
        {
            Element = Xml.Element(GetType().Name.ToLowerInvariant(), @namespace);
            To = to;
            From = from;
            foreach (XmlElement e in data)
            {
                if (e != null)
                    Element.Child(e);
            }
        }

        protected Stanza(XmlElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            this.Element = element;
        }

        public override string ToString()
        {
            //return element.ToXmlString();
            return Element.OuterXml;
        }

        protected XmlElement GetNode(params string[] nodeList)
        {
            return GetNode(Element, 0, nodeList);
        }

        private XmlElement GetNode(XmlElement node, int depth, params string[] nodeList)
        {
            XmlElement child = node[nodeList[depth]];
            return child == null || depth == nodeList.Length - 1 ? child : GetNode(child, depth + 1, nodeList);
        }
    }
}
