using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Linq;

namespace EmuWarface.Xmpp.Query
{
    public static class ClanKick
    {
        [Query(IqType.Get, "clan_kick")]
        public static void ClanKickSerializer(Client client, Iq iq)
        {
            if (client.Profile == null || client.Profile.ClanId == 0)
                throw new InvalidOperationException();

            var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={client.Profile.Id} AND clan_role=1");
            if (db.Rows.Count != 1)
                throw new QueryException(1);

            ulong target_id = ulong.Parse(iq.Query.GetAttribute("profile_id"));
            ulong clan_id = (ulong)db.Rows[0]["clan_id"];

            var db_clan_members = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE clan_id={clan_id} AND profile_id={target_id}");
            if (db_clan_members.Rows.Count != 1)
                throw new QueryException(1);

            //TODO gameroom clanwar kick

            SQL.Query($"DELETE FROM emu_clan_members WHERE clan_id={clan_id} AND profile_id={target_id}");

            //var notif = new Notification(NotificationType.Message, true, 9999, Notification.MessageNotificationSerialize("@clans_you_was_kicked"));
            var notif = Notification.MessageNotification("@clans_you_was_kicked");

            Client target = null;
            lock (Server.Clients)
            {
                target = Server.Clients.FirstOrDefault(x => x.ProfileId == target_id);
            }
            if (target != null)
            {
                Notification.SyncNotifications(target, notif);
                Clan.ClanInfo(target);

                target.Profile.Room?.GetExtension<GameRoomCore>()?.Update();
                target.Profile.Room?.Update();
            }

            Clan.ClanMembersUpdated(clan_id, target_id);

            client.QueryResult(iq);
        }
    }
}
