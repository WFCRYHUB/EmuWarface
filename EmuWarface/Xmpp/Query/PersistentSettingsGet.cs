using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Data;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class PersistentSettingsGet
    {
        [Query(IqType.Get, "persistent_settings_get")]
        public static void PersistentSettingsGetSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var db = SQL.QueryRead($"SELECT * FROM emu_persistent_settings WHERE profile_id={client.ProfileId}");

            XmlElement persistent_settings_get = Xml.Element(iq.Query.LocalName);

            foreach (DataRow property in db.Rows)
            {
                string type = property["type"].ToString();
                string name = property["name"].ToString();
                string value = property["value"].ToString();

                if (persistent_settings_get[type] == null)
                    persistent_settings_get.Child(Xml.Element(type));

                persistent_settings_get[type].Attr(name, value);
            }

            iq.SetQuery(persistent_settings_get);
            client.QueryResult(iq);
        }
    }
}
