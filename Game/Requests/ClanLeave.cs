using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class ClanLeave
    {
        [Query(IqType.Get, "clan_leave")]
        public static void ClanLeaveSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={client.ProfileId}");
            if (db.Rows.Count != 1) throw new QueryException(1);

            ulong clan_id = (ulong)db.Rows[0]["clan_id"];
            SQL.Query($"DELETE FROM emu_clan_members WHERE profile_id={client.ProfileId}");

            Clan.ClanInfo(client);

            client.Profile.Room?.GetExtension<GameRoomCore>()?.Update();
            client.Profile.Room?.Update();

            var clan_creation_item = client.Profile.Items.FirstOrDefault(x => x.Name == "clan_creation_item_01");
            if (clan_creation_item != null)
            {
                clan_creation_item.Delete();
                client.Profile.Items.Remove(clan_creation_item);
            }

            var db_clan_members = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE clan_id={clan_id}");
            if (db_clan_members.Rows.Count == 0)
            {
                SQL.Query($"DELETE FROM emu_clans WHERE clan_id={clan_id}");
                client.QueryResult(iq);
                return;
            }

            if ((ClanRole)db.Rows[0]["clan_role"] == ClanRole.Master)
            {
                ulong new_master_id = (ulong)db_clan_members.Rows[0]["profile_id"];
                SQL.Query($"UPDATE emu_clan_members SET clan_role=1 WHERE clan_id={clan_id} AND profile_id={new_master_id}");

                Clan.ClanMembersUpdated(clan_id, new_master_id);

                //var notif_master = new Notification(NotificationType.Message, true, 9999, Notification.MessageNotificationSerialize("@clans_you_are_promoted_to_master"));
                var notif_master = Notification.MessageNotification("@clans_you_are_promoted_to_master");
                Notification.SyncNotifications(new_master_id, notif_master);

                Clan.ClanMasterBannerUpdated(clan_id, new_master_id);
            }
            //else
            //{
                Clan.ClanMembersUpdated(clan_id, client.ProfileId/*, new List<ulong> { profile_id }*/);
            //}

            //TODO кик из комнаты клановой игры
            client.QueryResult(iq);
        }
    }
}
