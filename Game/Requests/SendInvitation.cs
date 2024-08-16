using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;

namespace EmuWarface.Game.Requests
{
    public static class SendInvitation
    {
        [Query(IqType.Get, "send_invitation")]
        public static void SendInvitationSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            NotificationType type = (NotificationType)int.Parse(iq.Query.GetAttribute("type"));
            ulong target_id = 0;

            switch (type)
            {
                case NotificationType.FriendInvite:
                    {
                        string target_nickname = iq.Query.GetAttribute("target");

                        target_id = Profile.GetProfileIdForNickname(target_nickname);
                        if (target_id == 0)
                            throw new QueryException(InviteStatus.TargetInvalid);

                        var db_dnd = SQL.QueryRead($"SELECT value FROM emu_persistent_settings WHERE profile_id={target_id} AND name='social.chat.dnd_mode'");
                        if (db_dnd.Rows.Count == 1 && db_dnd.Rows[0][0].ToString() == "1")
                            throw new QueryException(InviteStatus.DoNotDisturb);

                        var friends = Friend.GetFriends(client.ProfileId);
                        var target_friends = Friend.GetFriends(target_id);

                        if (friends.Contains(target_id) || target_friends.Contains(client.ProfileId))
                            throw new QueryException(InviteStatus.Duplicate);

                        if (friends.Count >= 50)
                            throw new QueryException(InviteStatus.LimitReached);

                        if (target_friends.Count >= 50)
                            throw new QueryException(InviteStatus.TargetLimitReached);

                        foreach (var notif in Notification.GetNotifications(target_id))
                        {
                            if (notif.Type == NotificationType.FriendInvite && notif.Element["initiator_info"].GetAttribute("profile_id") == client.ProfileId.ToString())
                                throw new QueryException(InviteStatus.InviteInProgress);
                        }

                        foreach (var notif in Notification.GetNotifications(client.ProfileId))
                        {
                            if (notif.Type == NotificationType.FriendInvite && notif.Element["initiator_info"].GetAttribute("profile_id") == target_id.ToString())
                                throw new QueryException(InviteStatus.ServiceError);
                        }

                        //var friend_notif = Notification.FriendInviteNotificationSerialize(target_id, string online_id, string profile_id, string nickname, int experience, string badge, string mark, string stripe, string clan_name = "");

                        //var invitation = Xml.Element("invitation").Attr("target", target_nickname);

                        //invitation.Child(Profile.GetInitiatorInfo(oProfile.ProfileId));

                        //var invitation_notif = new Notification(NotificationType.FriendInvite, true, 9999999, invitation);
                        var invitation_notif = Notification.FriendInvitationNotification(client.ProfileId, target_nickname);
                        Notification.AddNotification(target_id, invitation_notif);
                        Notification.SyncNotifications(target_id, invitation_notif);
                    }
                    break;
                case NotificationType.ClanInvite:
                    {
                        target_id = ulong.Parse(iq.Query.GetAttribute("target_id"));
                        if (target_id == 0)
                            throw new QueryException(InviteStatus.TargetInvalid);

                        Jid target_online_id = Profile.GetOnlineId(target_id);
                        if (target_online_id == null)
                            throw new QueryException(InviteStatus.UserOffline);

                        //TODO
                        var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={client.ProfileId}");
                        ClanRole clan_role = (ClanRole)db.Rows[0]["clan_role"];
                        ulong clan_id = (ulong)db.Rows[0]["clan_id"];

                        if (clan_role == ClanRole.Regular)
                            throw new QueryException(InviteStatus.NoPermission);

                        if (SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={target_id}").Rows.Count != 0)
                            throw new QueryException(InviteStatus.AlreadyClanMember);

                        var db_clan_members = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE clan_id={clan_id}");
                        if (db_clan_members.Rows.Count >= 50)
                            throw new QueryException(InviteStatus.LimitReached);

                        foreach (var notif in Notification.GetNotifications(target_id))
                        {
                            if (notif.Type == NotificationType.ClanInvite && notif.Element.GetAttribute("clan_id") == clan_id.ToString())
                                throw new QueryException(InviteStatus.InviteInProgress);
                        }

                        /*var invitation_clan = Xml.Element("invitation")
                            .Attr("target_id", target_id)
                            .Attr("clan_id", clan_id);

                        invitation_clan.Child(Profile.GetInitiatorInfo(oProfile.ProfileId));*/

                        var notif_invitation_clan = Notification.ClanInvitationNotification(client.ProfileId, target_id, clan_id);
                        Notification.AddNotification(target_id, notif_invitation_clan);
                        Notification.SyncNotifications(target_id, notif_invitation_clan);
                    }
                    break;
            }

            client.QueryResult(iq);
        }
    }
}