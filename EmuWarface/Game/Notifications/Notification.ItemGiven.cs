using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        //<notif id='3177524459' type='256' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
        //<give_item name='smg12_shop' offer_type='Permanent' notify='1'>
        //<sponsor_item/>
        //</give_item>
        //</notif>

        public static Notification GiveItemNotification(string item_name, string offer_type, bool notify, long seconds = 0, int quantity = 0)
        {
            var notif = Xml.Element("give_item")
                .Attr("name", item_name)
                .Attr("offer_type", offer_type)
                .Attr("notify", notify ? "1" : "0");

            if (offer_type == "Expiration")
                notif.Attr("extended_time", seconds / 3600);

            if (offer_type == "Consumable")
                notif.Attr("consumables_count", quantity);

            return new Notification
            {
                Element = notif,
                Type = NotificationType.ItemGiven,
                SecondsLeftToExpire = 36000
            };
        }
    }
}
