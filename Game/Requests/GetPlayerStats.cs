using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GetPlayerStats
    {
        [Query(IqType.Get, "get_player_stats")]
        public static void GetPlayerStatsSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            //XmlElement get_player_stats = Xml.Element(iq.Query.LocalName);

            /*client.Profile.Stats.ForEach(stat => 
            {
                if(stat.Stat != "player_wpn_usage")
                    get_player_stats.Child(stat.Serialize());
            });

            get_player_stats.Child(PlayerStat.GetPlayerStats(client.Profile.Stats));*/

            iq.SetQuery(PlayerStat.GetPlayerStats(client.Profile.Stats));
            client.QueryResult(iq);
        }
    }
}
