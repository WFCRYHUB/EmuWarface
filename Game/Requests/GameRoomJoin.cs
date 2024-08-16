using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
	public static class SetCurrenGameRoomJointClass
	{
        [Query(IqType.Get, "gameroom_join")]
        public static void GameRoomJoin(Client client, Iq iq)
		{
			if (client.Profile == null)
				throw new InvalidOperationException();

            //if (client.Profile.RoomPlayer != null)
            //    client.Profile.RoomPlayer.Room?.OnPlayerLeft(client);

            var room_id = long.Parse(iq.Query.GetAttribute("room_id"));
            var room    = client.Channel.Rooms.FirstOrDefault(room => room.Id == room_id);

            if (room == null)
                throw new QueryException(10);

            var rCore           = room.GetExtension<GameRoomCore>();
            var rCustomParams   = room.GetExtension<GameRoomCustomParams>();

            if (rCore.LeftPlayers.ContainsKey(client.ProfileId) && 
                rCore.LeftPlayers[client.ProfileId] != RoomPlayerRemoveReason.Left && 
                rCore.LeftPlayers[client.ProfileId] != RoomPlayerRemoveReason.KickTimeout &&
                rCore.LeftPlayers[client.ProfileId] != RoomPlayerRemoveReason.KickClan)
                throw new QueryException(2);

            if (room.IsPrivate)
                throw new QueryException(10);

            if (rCustomParams.GetCurrentRestriction("join_in_the_process") == "0" && !rCore.InvitedPlayers.Contains(client.ProfileId))
                throw new QueryException(10);

            if (room.Type == RoomType.PvP_ClanWar && client.Profile.ClanId == 0)
                throw new QueryException(13);

            if (rCore.Players.Count >= int.Parse(rCustomParams.GetCurrentRestriction("max_players")))
                throw new QueryException(4);

            room.JoinPlayer(client, iq.Query.GetAttribute("group_id"));

            if(room.Type == RoomType.PvP_Rating && rCore.PlayersReserved.Count == 0)
            {
                var dedicated = room.GetReadyDedicated();

                if (dedicated == null)
                {
                    EmuExtensions.Delay(5).ContinueWith(task =>
                    {
                        foreach (var player in rCore.Players.ToList())
                        {
                            room.KickPlayer(player, RoomPlayerRemoveReason.KickRankedGameCouldnotStart);
                            return;
                        }
                    });
                }
                else
                {
                    room.MissionLoad(dedicated);
                }
            }

            //TODO test какие бывает code
            XmlElement gameroom_join = Xml.Element(iq.Query.LocalName)
                .Attr("room_id", room_id)
                .Attr("code", "0");

            iq.SetQuery(gameroom_join.Child(room.Serialize(true)));
            client.QueryResult(iq);
		}
	}
}
