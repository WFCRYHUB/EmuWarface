using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        //<notif id='xxx' type='xxx' confirmation='0' from_jid='masterserver@warface/pve_059_r3' message=''>
        //<achievement achievement_id='xxx' progress='xxx' completion_time='xxx'/>
        //</notif>

        public static Notification AchievementNotification(uint achiev_id, int progress, long completion_time)
        {
            return new Notification
            {
                Element = Xml.Element("achievement")
                    .Attr("achievement_id", achiev_id)
                    .Attr("progress",       progress)
                    .Attr("completion_time", completion_time),
                Type = NotificationType.Achivement
            };
        }
    }
}
