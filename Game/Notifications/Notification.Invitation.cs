using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using System.Xml;

namespace EmuWarface.Game.Notifications
{
    public partial class Notification
    {
        //  <notif id="6016331252" type="64" confirmation="1" from_jid="masterserver@warface/pve_054_r1" message="">
        //    <invitation target="Покажите" >
        //      <initiator_info online_id="747193555@warface/GameClient" profile_id="25785570" is_online="1" name="МрЛоки1965Герда" clan_name="" experience="0" badge="4294967295" mark="4294967295" stripe="4294967295" />
        //    </invitation>
        //  </notif>

        public const long InvitationExpireSeconds = 604800;

        public static Notification FriendInvitationNotification(ulong initiator_id, string target_nickname)
        {
            var notif = InvitationNotification(initiator_id);

            notif.Element.Attr("target", target_nickname);
            notif.Type = NotificationType.FriendInvite;

            return notif;
        }

        public static Notification ClanInvitationNotification(ulong initiator_id, ulong target_id, ulong clan_id)
        {
            var notif = InvitationNotification(initiator_id);

            notif.Element.Attr("target_id", target_id);
            notif.Element.Attr("clan_id", clan_id);
            notif.Type = NotificationType.ClanInvite;

            return notif;
        }

        public static Notification InvitationResultNotification(ulong profile_id, NotificationType type, string result)
        {
            Profile profile = Profile.GetProfile(profile_id);
            var online_id   = Profile.GetOnlineId(profile_id);
            var status      = Profile.GetOnlineStatus(profile_id);

            return InvitationResultNotification(profile, online_id, status, type, result);
        }

        public static Notification InvitationResultNotification(Client client, NotificationType type, string result)
        {
            return InvitationResultNotification(client.Profile, client.Jid.ToString(), client.Presence, type, result);
        }

        private static Notification InvitationResultNotification(Profile profile, string online_id, PlayerStatus status, NotificationType type, string result)
        {
            XmlElement invite_result = Xml.Element("invite_result")
                .Attr("profile_id", profile.Id)
                .Attr("jid",        online_id)
                .Attr("nickname",   profile.Nickname)
                .Attr("status",     (int)status)
                .Attr("location",   "")
                .Attr("experience", profile.Experience)
                .Attr("result",     result) // warface думает иначе)
                .Attr("invite_date", "0");

            return new Notification
            {
                Type = type,
                Element = invite_result,
                SecondsLeftToExpire = InvitationExpireSeconds,
            };
        }

        private static Notification InvitationNotification(ulong initiator_id)
        {
            XmlElement invitation = Xml.Element("invitation")
                .Child(Profile.GetInitiatorInfo(initiator_id));

            return new Notification
            {
                Element = invitation,
                SecondsLeftToExpire = InvitationExpireSeconds
            };
        }
    }
}
