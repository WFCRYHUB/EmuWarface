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

        public static Notification GiveMoneyNotification(string currency, int amount, bool notify)
        {
            var notif = Xml.Element("give_money")
                .Attr("currency", currency)
                .Attr("type", "0")
                .Attr("amount", amount)
                .Attr("notify", notify ? "1" : "0");

            return new Notification
            {
                Element = notif,
                Type = NotificationType.MoneyGiven,
                SecondsLeftToExpire = 36000
            };
        }
    }
}
