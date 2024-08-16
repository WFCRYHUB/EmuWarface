using EmuWarface.Core;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    class SendAnticheatReport
    {
        //<send_anticheat_report session_id="1">
        //<cheat profile_id="19" type="rwi_hit_validation" score="700" calls="70" description="" />
        //<cheat profile_id="19" type="rwi_backface_hit_validation" score="100" calls="50" description="" />
        //</send_anticheat_report>

        [Query(IqType.Get, "send_anticheat_report")]
        public static void Serializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            var q = iq.Query;

            foreach (XmlElement cheat in q.GetElementsByTagName("cheat"))
            {
                var profile_id = ulong.Parse(cheat.GetAttribute("profile_id"));

                if (profile_id == 0)
                {
                    //API.SendAdmins("[Cheat] " + cheat.OuterXml);
                    continue;
                }

                MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_anticheat_report (`profile_id`, `type`, `score`, `calls`) VALUES " +
                   $"(@profile_id, @type, @score, @calls);");

                cmd.Parameters.AddWithValue("@profile_id", profile_id);
                cmd.Parameters.AddWithValue("@type",    cheat.GetAttribute("type"));
                cmd.Parameters.AddWithValue("@score",   uint.Parse(cheat.GetAttribute("score")));
                cmd.Parameters.AddWithValue("@calls",   uint.Parse(cheat.GetAttribute("calls")));

                SQL.Query(cmd);
            }

            client.QueryResult(iq.SetQuery(Xml.Element("send_anticheat_report")));
        }
    }
}
