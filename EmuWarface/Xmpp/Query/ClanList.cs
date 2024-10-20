using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using System;
using System.Data;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class ClanList
    {
        [Query(IqType.Get, "clan_list")]
        public static void ClanListSerializer(Client client, Iq iq)
        {
            ulong clan_id = client.Profile.ClanId;

            XmlElement clan_list = Xml.Element("clan_list");
            XmlElement clan_performance = Xml.Element("clan_performance").Attr("position", Clan.GetClanPosition(clan_id));

            foreach (var top_clan in Clan.ClanList.Take(10))
            {
                var db_clan = SQL.QueryRead($"SELECT * FROM emu_clans WHERE clan_id={top_clan.Key}").Rows[0];
                var db_members = SQL.QueryRead($"SELECT clan_id FROM emu_clan_members WHERE clan_id='{db_clan["clan_id"]}'");

                XmlElement clan = Xml.Element("clan")
                    .Attr("name", db_clan["name"])
                    .Attr("master", Clan.GetClanLeader(Convert.ToUInt64(db_clan["clan_id"])))
                    .Attr("clan_points", top_clan.Value)
                    .Attr("members", db_members.Rows.Count);

                clan_performance.Child(clan);
            }
            clan_list.Child(clan_performance);

            iq.SetQuery(clan_list);
            client.QueryResult(iq);
        }
    }
}
