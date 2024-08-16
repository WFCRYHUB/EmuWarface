using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        //<notif id='xxx' type='xxx' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
        //<give_random_box name='box' notify='1'>
        //<purchased_item>
        //<profile_item/>
        //</purchased_item>
        //</give_item>
        //</notif>

        public static Notification GiveRandomBoxNotification(string box_name, XmlElement purchased_item)
        {
            var notif = Xml.Element("give_random_box")
                .Attr("name",       box_name)
                .Attr("notify",     "1");

            notif.Child(purchased_item);

            return new Notification
            {
                Element = notif,
                Type    = NotificationType.RandomBoxGiven,
                SecondsLeftToExpire = 36000
            };
        }
    }
}
