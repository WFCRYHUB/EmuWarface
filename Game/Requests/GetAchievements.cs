using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GetAchievements
    {
        [Query(IqType.Get, "get_achievements")]
        public static void GetAchievementsSerializer(Client client, Iq iq)
        {
            var q = iq.Query;

            ulong target_id = 0;

            if (client.IsDedicated)
            {
                target_id = ulong.Parse(q.FirstChild.Attributes["profile_id"].InnerText);
            }
            else
            {
                target_id = client.ProfileId;
            }

            if (target_id == 0)
                throw new InvalidOperationException();

            XmlElement response = Xml.Element("get_achievements");
            XmlElement achievement = Xml.Element("achievement").Attr("profile_id", target_id);

            lock (Server.Clients)
            {
                Server.Clients.FirstOrDefault(x => x.ProfileId == target_id)?.Profile?.Achievements?.ForEach(x => achievement.Child(x.Serialize()));
            }

            response.Child(achievement);

            iq.SetQuery(response);
            client.QueryResult(iq);
        }
    }
}
