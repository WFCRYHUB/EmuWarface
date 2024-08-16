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
    public static class GameRoomSetPlayer
    {
        [Query(IqType.Get, "gameroom_setplayer")]
        public static void GameRoomSetPlayerSerializer(Client client, Iq iq)
        {
            //TODO прислать пакет когда не в комнате
            if (client.Profile == null || client.Profile.Room == null)
                throw new QueryException(1);

            var q = iq.Query;

            var roomPlayer = client.Profile.RoomPlayer;
            var room = client.Profile.RoomPlayer.Room;

            var rCore   = room.GetExtension<GameRoomCore>();

            if (rCore == null)
                throw new QueryException(1);


            //TODO если послать несколько классов
            //TODO проверять количество игроков в команде
            roomPlayer.TeamId = EmuExtensions.ParseEnum<Team>(q.GetAttribute("team_id"));
            roomPlayer.Status = EmuExtensions.ParseEnum<RoomPlayerStatus>(q.GetAttribute("status"));
            client.Profile.CurrentClass = EmuExtensions.ParseEnum<ClassId>(q.GetAttribute("class_id"));
            //oProfile.Profile.Update();

            room.ValidateClasses();

//#if !DEBUGLOCAL
            if (room.Type != RoomType.PvE_Autostart && room.Type != RoomType.PvP_Autostart && room.Type != RoomType.PvP_Rating)
            {
                var rMaster = room.GetExtension<GameRoomMaster>();

                var master = rMaster.Client;
                EmuExtensions.Delay(10).ContinueWith(task => room.StartMasterLoseTimer(master));
            }
            //#endif


            XmlElement gameroom_setplayer = Xml.Element(iq.Query.LocalName);

            iq.SetQuery(gameroom_setplayer.Child(room.Serialize().Child(rCore.Serialize())));
            client.QueryResult(iq);
        }
    }
}
