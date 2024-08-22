using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class GameRoomAskServer
    {
        [Query(IqType.Get, "gameroom_askserver")]
        public static void GameRoomAskServerSerializer(Client client, Iq iq)
        {
            if (client.Profile == null || client.Profile.Room == null)
                throw new QueryException(1);

            var q = iq.Query;

            var roomPlayer = client.Profile.RoomPlayer;
            var room = client.Profile.RoomPlayer.Room;

            var rCore = room.GetExtension<GameRoomCore>();
            var rMaster = room.GetExtension<GameRoomMaster>();
            var rSession = room.GetExtension<GameRoomSession>();

            if (rCore == null || rMaster == null || rSession == null || rMaster.Client != client)
                throw new QueryException(1);

            if (rSession.Status != SessionStatus.None || rSession.Id != 0)
                throw new QueryException(7);

            lock (rCore.Players)
            {
                if (rCore.MinReadyPlayers > rCore.Players.Count(x => x.Profile.RoomPlayer.Status == RoomPlayerStatus.Ready))
                    throw new QueryException(3);

                if (room.Type == RoomType.PvP_Public)
                {
                    if (rCore.MinReadyPlayers / 2 > rCore.Players.Count(x => x.Profile.RoomPlayer.Status == RoomPlayerStatus.Ready && x.Profile.RoomPlayer.TeamId == Team.Warface))
                        throw new QueryException(3);

                    if (rCore.MinReadyPlayers / 2 > rCore.Players.Count(x => x.Profile.RoomPlayer.Status == RoomPlayerStatus.Ready && x.Profile.RoomPlayer.TeamId == Team.Blackwood))
                        throw new QueryException(3);
                }
            }

            Log.Info("[GameRoom] AskServer dedicateds available: {0} (room_id: {1})", Server.Dedicateds.Count, room.Id);

            var dedicated = room.GetReadyDedicated();

            if (dedicated == null)
                throw new QueryException(7);

            room.MissionLoad(dedicated);

            //if (!DedicatedSystem.StartSession(room))
            //    throw new QueryException(8);

            client.QueryResult(iq);
        }
    }
}
