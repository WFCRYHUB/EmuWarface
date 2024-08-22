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
    public static class GameRoomSwitchTeams
    {
        [Query(IqType.Get, "gameroom_switchteams")]
        public static void GameRoomSwitchTeamsSerializer(Client client, Iq iq)
        {
            //TODO прислать пакет когда не в комнате
            if (client.Profile == null || client.Profile.Room == null)
                throw new QueryException(1);

            var q = iq.Query;

            var roomPlayer = client.Profile.RoomPlayer;
            var room = client.Profile.RoomPlayer.Room;

            var rCore = room.GetExtension<GameRoomCore>();
            var rMaster = room.GetExtension<GameRoomMaster>();

            if (rCore == null || rMaster == null || rMaster.Client != client)
                throw new QueryException(1);

            rCore.TeamsSwitched = !rCore.TeamsSwitched;

            var gameroom_switchteams = Xml.Element("gameroom_switchteams");
            gameroom_switchteams.Child(room.Serialize().Child(rCore.Serialize()));

            iq.SetQuery(gameroom_switchteams);
            client.QueryResult(iq);
        }
    }
}
