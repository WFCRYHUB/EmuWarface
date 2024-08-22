using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification 
    { 
        //<notif id='3177528405' type='262144' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
        //<message data='@Medic_unlocked'/>
        //</notif>

        private static int _seed_announcement_id = 0;

        public static Notification MessageNotification(string data, long secondsLeft = 172800)
        {
            var notif = Xml.Element("message").Attr("data", data);

            return new Notification
            {
                Element     = notif,
                Type        = NotificationType.Message,
                SecondsLeftToExpire = secondsLeft,
            };
        }

        public static Notification AnnouncementNotification(string message, long secondsLeft = 172800)
        {
            var notif = Xml.Element("announcement")
                .Attr("id",             ++_seed_announcement_id)
                .Attr("message",        message)
                .Attr("server",         "emuwarface")
                .Attr("channel",        "emuwarface")
                .Attr("repeat_time",    "0")
                .Attr("frequency",      long.MaxValue)
                .Attr("place",          "1");

            return new Notification
            {
                Element = notif,
                Type = NotificationType.Announcement,
                SecondsLeftToExpire = secondsLeft,
            };
        }

        public static Notification CongratulationMessageNotification(string data, long secondsLeft = 172800)
        {
            var notif = Xml.Element("message").Attr("data", data);

            return new Notification
            {
                Element     = notif,
                Type        = NotificationType.CongratulationMessage,
                SecondsLeftToExpire = secondsLeft
            };
        }
    }
}
