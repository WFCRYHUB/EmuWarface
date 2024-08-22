using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        //<notif id='xxx' type='xxx' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
        //<item_deleted profile_item_id='xxx'/>
        //</notif>

        public static Notification DeleteItemNotification(ulong item_id)
        {
            return new Notification
            {
                Element = Xml.Element("item_deleted").Attr("profile_item_id", item_id),
                Type = NotificationType.ItemDeleted,
            };
        }
    }
}
