using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using System;
using System.Data;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class UpdateAchievements
    {
        //<update_achievements xmlns="">
        //<achievement profile_id="12">
        //<chunk achievement_id="51" progress="12635" completion_time="0" />
        //<chunk achievement_id="52" progress="12635" completion_time="0" />
        //<chunk achievement_id="53" progress="12635" completion_time="0" />
        //</achievement>
        //<achievement profile_id="19">
        //<chunk achievement_id="51" progress="7291" completion_time="0" />
        //<chunk achievement_id="52" progress="7291" completion_time="0" />
        //<chunk achievement_id="53" progress="7291" completion_time="0" />
        //</achievement>
        //</update_achievements>

        [Query(IqType.Get, "update_achievements")]
        public static void UpdateAchievementsSerializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            var q = iq.Query;

            foreach (XmlElement achievement in q.ChildNodes)
            {
                Profile profile = Profile.GetProfile(ulong.Parse(achievement.GetAttribute("profile_id")));
                if (profile == null)
                    continue;

                foreach (XmlElement chunk in achievement.ChildNodes)
                {
                    uint achievement_id = uint.Parse(chunk.GetAttribute("achievement_id"));
                    int progress = int.Parse(chunk.GetAttribute("progress"));

                    long completion_time = long.Parse(chunk.GetAttribute("completion_time"));

                    Achievement.SetAchiev(profile.Id, achievement_id, progress, completion_time);
                    //var achiev = profile.Achievements.FirstOrDefault(x => x.AchievementId == chunk.GetAttribute("achievement_id"));
                }
            }

            iq.SetQuery(Xml.Element("update_achievements"));
            client.QueryResult(iq);
        }
    }
}

