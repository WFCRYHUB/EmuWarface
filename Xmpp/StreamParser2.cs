using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace EmuWarface.Xmpp
{
    public class StreamParser2
    {
        private enum State
        {
            TEXT,
            TAG_NAME,
            ATTR_NAME,
            ATTR_QUOT,
            ATTR_VALUE,
            SKIP_DECLORATION,
        }

        private enum ElementType
        {
            OPEN,
            SELF_CLOSED,
            CONTAINING,
        }

        private State _state = State.TEXT;
        private ElementType _elementType = ElementType.OPEN;
        private string _elementName;
        private string _text;
        private Dictionary<string, string> _attrs = new();
        private string _attrName;
        private string _attrValue;
        
        private XElement? _element;

        public event EventHandler OnStreamStart;
        public event EventHandler OnStreamEnd;
        public event EventHandler OnError;
        public event EventHandler<XmlElement> OnStanza;

        private void onStartElement(string name, Dictionary<string, string> attrs)
        {
            if (this._element == null && name == "stream:stream") {
                OnStreamStart?.Invoke(this, null);
                //this.emit("streamStart", attrs);
            } 
            else
            {
                if (this._element == null)
                {
                    var xname = attrs.ContainsKey("xmlns") ? (XNamespace)attrs["xmlns"] + name : name;
                    this._element = new XElement(xname);
                    //this._element = new XElement(name/*, attrs.ContainsKey("xmlns") ? attrs["xmlns"] : null*/);
                    foreach(var attr in attrs)
                    {
                        this._element.SetAttributeValue(attr.Key, attr.Value);
                    }
                    //this._element = new XmlElement(name, attrs);
                }
                else
                {
                    var xname = attrs.ContainsKey("xmlns") ? (XNamespace)attrs["xmlns"] + name : name;
                    var e = new XElement(xname);
                    foreach (var attr in attrs)
                    {
                        e.SetAttributeValue(attr.Key, attr.Value);
                    }
                    this._element.Add(e);
                    //this._parent = this._element;
                    this._element = e;
                    //this._element = this._element.cnode(new Element(name, attrs));
                }
            }
        }

        private void onEndElement(string name)
        {
            if (this._element == null && name == "stream:stream") {
                OnStreamEnd?.Invoke(this, null);
                // this.emit("streamEnd");
            }
            else if (this._element == null || name != this._element.Name.LocalName)
            {
                OnError?.Invoke(this, null);
                //this.emit("error");
            }
            else if (this._element.Parent == null)
            {
                var temp = Xml.Parse(this._element.ToString(SaveOptions.DisableFormatting));
                OnStanza?.Invoke(this, temp);
                // this.emit("stanza", this._element);
                this._element = null;
            }
            else
            {
                this._element = this._element.Parent;
            }
        }

        private void onText(string text)
        {
            if (this._element != null) 
                this._element.Add(text);
        }

        public void write(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            for (int i = 0; i < data.Length; i++)
            {
                char c = data[i];
                switch (this._state)
                {
                    case State.TEXT:
                        {
                            if (c == '<')
                            {
                                if (!string.IsNullOrEmpty(this._text))
                                {
                                    this.onText(this._text);
                                    this._text = "";
                                }
                                this._state = State.TAG_NAME;
                                break;
                            }
                            this._text += c;
                            break;
                        }
                    case State.TAG_NAME:
                        {
                            if (c == ' ' || c == '\t')
                            {
                                this._state = State.ATTR_NAME;
                                break;
                            }
                            if (c == '/')
                            {
                                if (string.IsNullOrEmpty(this._elementName))
                                    this._elementType = ElementType.SELF_CLOSED;
                                else 
                                    this._elementType = ElementType.CONTAINING;
                                break;
                            }
                            if (c == '>')
                            {
                                switch (this._elementType)
                                {
                                    case ElementType.OPEN:
                                        this.onStartElement(
                                            this._elementName,
                                            this._attrs
                                        );
                                        break;
                                    case ElementType.SELF_CLOSED:
                                        this.onEndElement(this._elementName);
                                        break;
                                    case ElementType.CONTAINING:
                                        this.onStartElement(
                                            this._elementName,
                                            this._attrs
                                        );
                                        this.onEndElement(this._elementName);
                                        break;
                                }
            
                                this._elementType = 0;
                                this._elementName = "";
                                this._attrs = new();
                                this._state = State.TEXT;
                                break;
                            }
                            if (c == '?')
                            {
                                this._state = State.SKIP_DECLORATION;
                                break;
                            }
                            this._elementName += c;
                            break;
                        }
                    case State.ATTR_NAME:
                        {
                            if (c == '=')
                            {
                                this._state = State.ATTR_QUOT;
                                break;
                            }
                            this._attrName += c;
                            break;
                        }
                    case State.ATTR_QUOT:
                        {
                            if (c == '\'' || c == '"')
                            {
                                this._state = State.ATTR_VALUE;
                                break;
                            }
                            break;
                        }
                    case State.ATTR_VALUE:
                        {
                            if (c == '\'' || c == '"')
                            {
                                this._attrs[this._attrName] = this._attrValue;
                                this._attrName = "";
                                this._attrValue = "";
                                this._state = State.TAG_NAME;
                                break;
                            }
                            this._attrValue += c;
                            break;
                        }
                    case State.SKIP_DECLORATION:
                        if (c == '>') this._state = State.TEXT;
                        break;
                }
            }
        }
    }
}
