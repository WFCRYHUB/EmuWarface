using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Xmpp.Query
{
    public class ClanSetMemberRole
    {
        [Query(IqType.Get, "clan_set_member_role")]
        public static void ClanSetMemberRoleSerializer(Client client, Iq iq)
        {
            //profile_id="5" role="2"
            if (client.Profile == null || client.Profile.ClanId == 0)
                throw new InvalidOperationException();

            ulong clan_id = client.Profile.ClanId;
            ulong target_id = ulong.Parse(iq.Query.GetAttribute("profile_id"));
            ClanRole setRole = (ClanRole)int.Parse(iq.Query.GetAttribute("role"));

            if (!Enum.IsDefined(typeof(ClanRole), setRole))
                throw new QueryException(1);

            if (client.Profile.ClanId != Clan.GetClanId(target_id))
                throw new QueryException(7);

            var db_master = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={client.ProfileId}");
            if (db_master.Rows.Count == 0 || (ClanRole)db_master.Rows[0]["clan_role"] != ClanRole.Master)
                throw new QueryException(7);

            SQL.Query($"UPDATE emu_clan_members SET clan_role={(int)setRole} WHERE profile_id={target_id} AND clan_id={clan_id}");

            string message = "@clans_you_are_demoted_to_regular";
            if (setRole == ClanRole.Master) message = "@clans_you_are_promoted_to_master";
            if (setRole == ClanRole.Officer) message = "@clans_you_are_promoted_to_Officer";

            //var notif = new Notification(NotificationType.Message, true, 9999, Notification.MessageNotificationSerialize(message));
            var notif = Notification.MessageNotification(message);
            Notification.AddNotification(target_id, notif);
            Notification.SyncNotifications(target_id, notif);

            //Для прежнего главы
            if (setRole == ClanRole.Master)
            {
                SQL.Query($"UPDATE emu_clan_members SET clan_role={(int)ClanRole.Officer} WHERE profile_id={client.ProfileId} AND clan_id={clan_id}");

                //var notif_master = new Notification(NotificationType.Message, true, 9999, Notification.MessageNotificationSerialize("@clans_you_are_promoted_to_officer"));
                var notif_master = Notification.MessageNotification("@clans_you_are_promoted_to_officer");
                Notification.SyncNotifications(client, notif_master);

                Clan.ClanMembersUpdated(clan_id, client.ProfileId, target_id);

                //TODO test
                Clan.ClanMasterBannerUpdated(clan_id, Profile.GetProfile(target_id));
            }
            else
            {
                Clan.ClanMembersUpdated(clan_id, target_id);
            }

            client.QueryResult(iq);
        }
    }
}
