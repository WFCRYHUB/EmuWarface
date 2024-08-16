using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class ConfirmNotification
    {
        [Query(IqType.Get, "confirm_notification")]
        public static void ConfirmNotificationSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            foreach (XmlElement node in iq.Query.ChildNodes)
            {
                ulong notif_id = ulong.Parse(node.GetAttribute("id"));
                if (notif_id == 0) continue;

                var notif = client.Profile.Notifications.FirstOrDefault(x => x.Id == notif_id);
                if (notif == null) continue;

                Notification.RemoveNotification(client.ProfileId, notif.Id);

                switch (notif.Type)
                {
                    //<invitation target="Покажите"><initiator_info online_id="" profile_id="25785570" is_online="0" name="МрЛоки1965Герда" clan_name="" experience="0" badge="4294967295" mark="4294967295" stripe="4294967295" /></invitation>
                    //<notif id="124" type="16"><confirmation result="0" status="9" location="В зале" /></notif>
                    case NotificationType.ClanInvite:
                    case NotificationType.FriendInvite:
                        {
                            var confirmation = node["confirmation"];
                            var type = notif.Type;

                            //Invitation.OnInvitationConfirm(client, notif.Type, notif.Element, node["confirmation"]);
                            string result = confirmation.GetAttribute("result");
                            var initiator_id = ulong.Parse(notif.Element["initiator_info"].GetAttribute("profile_id"));

                            //TODO errors id
                            //if accept
                            if (result == "0")
                            {
                                if (type == NotificationType.ClanInvite)
                                {
                                    ulong clan_id = ulong.Parse(notif.Element.GetAttribute("clan_id"));

                                    var db_clan = SQL.QueryRead($"SELECT * FROM emu_clans WHERE clan_id={clan_id}");
                                    if (db_clan.Rows.Count == 0)
                                        throw new QueryException(1);

                                    var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={client.ProfileId}");
                                    if (db.Rows.Count != 0)
                                        throw new QueryException(1);

                                    var db_clan_members = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE clan_id={clan_id}");
                                    if (db_clan_members.Rows.Count >= 50)
                                        throw new QueryException(1);

                                    SQL.Query($"INSERT INTO emu_clan_members (`profile_id`, `clan_id`, `invite_date`) VALUES ({client.ProfileId}, {clan_id}, {DateTimeOffset.UtcNow.ToUnixTimeSeconds()})");

                                    Clan.ClanInfo(client);
                                    Clan.ClanMembersUpdated(clan_id, client.ProfileId);
                                }
                                else
                                {
                                    if (Friend.GetFriends(initiator_id).Count >= 50)
                                        throw new QueryException(1);

                                    if (Friend.GetFriends(client.ProfileId).Count >= 50)
                                        throw new QueryException(1);

                                    Friend.AddFriend(client.ProfileId, initiator_id);

                                    Notification notif_result2 = Notification.InvitationResultNotification(initiator_id, type == NotificationType.ClanInvite ? NotificationType.ClanInviteResult : NotificationType.FriendInviteResult, result);

                                    Notification.AddNotification(client.ProfileId, notif_result2);
                                    Notification.SyncNotifications(client, notif_result2);
                                    Notification.RemoveNotification(client.ProfileId, notif_result2.Id);
                                }
                            }

                            Notification notif_result = Notification.InvitationResultNotification(client, type == NotificationType.ClanInvite ? NotificationType.ClanInviteResult : NotificationType.FriendInviteResult, result);

                            Notification.AddNotification(initiator_id, notif_result);
                            Notification.SyncNotifications(initiator_id, notif_result);
                            Notification.RemoveNotification(initiator_id, notif_result.Id); //warface обновили UI, из-за ID приходится вставлять в бд(
                        }
                        break;
                }

                //Notification.RemoveNotification(client.ProfileId, notif.Id);
            }

            iq.SetQuery(Xml.Element("confirm_notification"));
            client.QueryResult(iq);
        }


    }
}
