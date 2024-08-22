using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        //<notif id='3177528405' type='262144' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
        //<new_rank_reached old_rank='0' new_rank='1'/>
        //</notif>

        public static Notification NewRankReachedNotification(int old_rank, int new_rank, long secondsLeft = 172800)
        {
            var notif = Xml.Element("new_rank_reached")
                .Attr("old_rank", old_rank)
                .Attr("new_rank", new_rank);

            return new Notification
            {
                Element = notif,
                Type = NotificationType.NewRankReached,
                SecondsLeftToExpire = secondsLeft,
            };
        }
    }
}
