using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GameRoomPromoteToHost
    {
        //<gameroom_promote_to_host new_host_profile_id="5" />

        [Query(IqType.Get, "gameroom_promote_to_host")]
        public static void GameRoomPromoteToHostSerializer(Client client, Iq iq)
        {
            if (client.Profile == null || client.Profile.Room == null)
                throw new QueryException(1);

            var q = iq.Query;
            var room = client.Profile.RoomPlayer?.Room;

            var rCore   = room.GetExtension<GameRoomCore>();
            var rMaster = room.GetExtension<GameRoomMaster>();

            if (rCore == null || rMaster == null || rMaster.Client != client)
                throw new QueryException(1);


            var new_host_profile_id = ulong.Parse(q.GetAttribute("new_host_profile_id"));

            var target = rCore.Players.FirstOrDefault(x => x.ProfileId == new_host_profile_id);
            if (target != null)
            {
                room.SetMaster(target);
            }

            var gameroom_promote_to_host = Xml.Element("gameroom_promote_to_host");
            //gameroom_promote_to_host.Child(room.Serialize().Child(room.Master.Serialize().Child(room.Core.Serialize())));

            iq.SetQuery(gameroom_promote_to_host);
            client.QueryResult(iq);
        }
    }
}
