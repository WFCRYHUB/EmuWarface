using EmuWarface.Core;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;

namespace EmuWarface.Game.Requests
{
    public static class AbuseReport
    {
        /*
         <abuse_report target="Скрим" type="cheat" comment="" />
         */

        [Query(IqType.Get, "abuse_report")]
        public static void AbuseReportSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_abuse_reports (`initiator`, `target`, `type`, `comment`) VALUES " +
                   $"(@initiator, @target, @type, @comment);");

            cmd.Parameters.AddWithValue("@initiator",  client.Profile.Nickname);
            cmd.Parameters.AddWithValue("@target",  q.GetAttribute("target"));
            cmd.Parameters.AddWithValue("@type",    q.GetAttribute("type"));
            cmd.Parameters.AddWithValue("@comment", q.GetAttribute("comment"));

            SQL.Query(cmd);

            client.QueryResult(iq.SetQuery(Xml.Element("abuse_report")));
        }
    }
}
