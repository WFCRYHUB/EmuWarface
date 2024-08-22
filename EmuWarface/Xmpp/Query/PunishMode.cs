using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmuWarface.Xmpp.Query
{
    public static class PunishMode
    {
        //<punish_mode profile_id="19" session_id="1" punish_mode="kick_anticheat" />

        [Query(IqType.Get, "punish_mode")]
        public static void Serializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            var q = iq.Query;

            var profile_id = ulong.Parse(q.GetAttribute("profile_id"));
            var punish_mode = q.GetAttribute("punish_mode");

            if (punish_mode == "kick_anticheat")
            {
                var room = client.Dedicated.Room;
                var rCore = room.GetExtension<GameRoomCore>();

                var player = rCore.Players.FirstOrDefault(x => x.ProfileId == profile_id);

                if (player == null)
                    return;

                room.KickPlayer(player, RoomPlayerRemoveReason.KickAntiCheat);

                MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_anticheat_punish_mode (`profile_id`, `punish_mode`) VALUES " +
                   $"(@profile_id, @punish_mode);");

                cmd.Parameters.AddWithValue("@profile_id", profile_id);
                cmd.Parameters.AddWithValue("@punish_mode", punish_mode);

                SQL.Query(cmd);
            }

            client.QueryResult(iq);
        }
    }
}
