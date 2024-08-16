using EmuWarface.Core;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class PersistentSettingsSet
    {
        [Query(IqType.Get, "persistent_settings_set")]
        public static void PersistentSettingsSetSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            XmlElement settings = iq.Query["settings"];
            foreach (XmlNode node in settings.ChildNodes)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    //TODO test
                    //Log.Info($"[PersistentSettings] {node.LocalName} {attr.Name} {attr.Value}");

                    MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_persistent_settings (profile_id, type, name, value) VALUES(@profile_id, @type, @name, @value) ON DUPLICATE KEY UPDATE value=@value");

                    cmd.Parameters.AddWithValue("@profile_id", client.ProfileId);
                    cmd.Parameters.AddWithValue("@type", node.LocalName);
                    cmd.Parameters.AddWithValue("@name", attr.Name);
                    cmd.Parameters.AddWithValue("@value", attr.Value);

                    SQL.Query(cmd);
                }
            }
            //<persistent_settings_set><settings><options social.chat.dnd_mode="1" /></settings></persistent_settings_set>
            //return iq;

            client.QueryResult(iq);
        }
    }
}
