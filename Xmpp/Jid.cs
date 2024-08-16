using System;
using System.Text;
using System.Text.RegularExpressions;

namespace EmuWarface.Xmpp
{
    public sealed class Jid
    {
        public string Domain
        {
            get;
            private set;
        }

        public string Node
        {
            get;
            private set;
        }

        public string Resource
        {
            get;
            private set;
        }

        public bool IsBareJid
        {
            get
            {
                return !string.IsNullOrEmpty(Node) &&
                    !string.IsNullOrEmpty(Domain) && string.IsNullOrEmpty(Resource);
            }
        }

        public bool IsFullJid
        {
            get
            {
                return !string.IsNullOrEmpty(Node) &&
                    !string.IsNullOrEmpty(Domain) && !string.IsNullOrEmpty(Resource);
            }
        }

        public Jid(string jid)
        {
            if (string.IsNullOrEmpty(jid))
                throw new ArgumentNullException("jid");
            Match m = Regex.Match(jid,
                "(?:(?<node>[^@]+)@)?(?<domain>[^/]+)(?:/(?<resource>.+))?");
            if (!m.Success)
                throw new ArgumentException("The argument is not a valid JID.");
            Domain = m.Groups["domain"].Value;
            Node = m.Groups["node"].Value;
            if (Node == string.Empty)
                Node = null;
            Resource = m.Groups["resource"].Value;
            if (Resource == string.Empty)
                Resource = null;
        }

        public Jid(string domain, string node, string resource = null)
        {
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException("domain");
            Domain = domain;
            Node = node;
            Resource = resource;
        }

        public static implicit operator Jid(string jid)
        {
            return new Jid(jid);
        }

        public Jid GetBareJid()
        {
            return new Jid(Domain, Node);
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            if (!string.IsNullOrEmpty(Node))
                b.Append(Node + "@");
            b.Append(Domain);
            if (!string.IsNullOrEmpty(Resource))
                b.Append("/" + Resource);
            return b.ToString();
        }

        /*public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Jid other = obj as Jid;
            if (other == null)
                return false;
            return Node == other.Node && Domain == other.Domain &&
                Resource == other.Resource;
        }
        */
        public static bool operator ==(Jid a, Jid b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a.Node == b.Node && a.Domain == b.Domain &&
                a.Resource == b.Resource;
        }

        public static bool operator !=(Jid a, Jid b)
        {
            return !(a == b);
        }
    }
}