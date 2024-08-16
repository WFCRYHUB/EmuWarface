using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Linq;

namespace EmuWarface.Game.Requests
{
    //<gameroom_reconnect room_id="1" profile_id="21" />

    public static class GameRoomReconnect
    {
        [Query(IqType.Get, "gameroom_reconnect")]
        public static void GameRoomReconnectSerializer(Client client, Iq iq)
        {
            //TODO
            throw new QueryException(1);

            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            long room_id = long.Parse(q.GetAttribute("room_id"));
            var room = client.Channel.Rooms.FirstOrDefault(x => x.Id == room_id);

            if (room == null)
                throw new QueryException(1);

            client.QueryResult(iq);
        }
    }
}
