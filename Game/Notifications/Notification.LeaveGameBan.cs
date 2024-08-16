using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        //<notif id='xxxx' type='4194304' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
        //<ProfileBan room_type='PvP_Autostart' ban_type='Progressive' ban_end_time='999' trial_end_time='3000' last_ban_index='1'/>
        //</notif>

        //ban_type: Progressive, Custom

        public static Notification LeaveGameBanNotification(ProfileBan profileBan)
        {
            var notif = Xml.Element("ProfileBan")
                 .Attr("room_type",             profileBan.RoomType)
                 .Attr("ban_type",              profileBan.BanType)
                 .Attr("ban_seconds_left",      profileBan.BanSecondsLeft)
                 .Attr("trial_seconds_left",    profileBan.TrialSecondsLeft)
                 .Attr("last_ban_index",        profileBan.LastBanIndex);

            return new Notification
            {
                Element = notif,
                Type = NotificationType.LeaveGameBan
            };
        }
    }
}
