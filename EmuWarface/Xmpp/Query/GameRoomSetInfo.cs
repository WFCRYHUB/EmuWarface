using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class GameRoomSetInfo
    {
        //<gameroom_setinfo by_mission_key='1' mission_key='b9f119cd-1c26-44fa-8093-805e6f4d9076'/>

        [Query(IqType.Get, "gameroom_setinfo")]
        public static void GameRoomSetInfoSerializer(Client client, Iq iq)
        {
            if (client.Profile == null || client.Profile.Room == null)
                throw new QueryException(1);

            var q = iq.Query;

            var roomPlayer = client.Profile.RoomPlayer;
            var room = client.Profile.RoomPlayer.Room;

            var rCore = room.GetExtension<GameRoomCore>();
            var rMaster = room.GetExtension<GameRoomMaster>();
            var rMission = room.GetExtension<GameRoomMission>();

            if (rCore == null || rMaster == null || rMaster.Client != client)
                throw new QueryException(1);

            var mission_key = q.GetAttribute("mission_key");

            if (mission_key == rMission.Key)
                throw new QueryException(1);

            Mission mission = Mission.GetMission(room.Type, mission_key);

            if (mission == null)
                throw new QueryException(1);

            if (!room.Type.ToString().Contains("PvE"))
                throw new QueryException(1);

            room.SetMission(mission);

            iq.SetQuery(Xml.Element("gameroom_setinfo").Child(room.Serialize().Child(rMission.Serialize())));
            client.QueryResult(iq);
        }
    }
}
