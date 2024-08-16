using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GameRoomOpen
    {
        /*
         <iq to='masterserver@warface/pvp_skilled_001' id='uid00000043' type='get' from='3@warface/GameClient' xmlns='jabber:client'>
<query xmlns='urn:cryonline:k01'>
<gameroom_open 
        room_name='Комната игрока 3333' 
        team_id='0' 
        status='0' 
        class_id='0' 
        group_id='' 
        room_type='2' 
        private='0' 
        mission='e4f56e59-97fb-4451-a137-7b44026d96b0' 
        friendly_fire='0' 
        enemy_outlines='0' 
        auto_team_balance='1' 
        join_in_the_process='1' 
        max_players='16' 
        round_limit='0' 
        preround_time='-1' 
        inventory_slot='34326183935' 
        locked_spectator_camera='0' 
        high_latency_autokick='1' 
        overtime_mode='0'>
<class_rifleman enabled='1' class_id='0'/>
<class_heavy enabled='1' class_id='1'/>
<class_engineer enabled='1' class_id='4'/>
<class_medic enabled='1' class_id='3'/>
<class_sniper enabled='1' class_id='2'/>
</gameroom_open>
</query>
</iq>
         */

        [Query(IqType.Get, "gameroom_open")]
        public static void GameRoomOpenSerializer(Client client, Iq iq)
        {
            //TODO проверять карту и канал на котором он создает

            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            client.Profile.Room?.LeftPlayer(client);
            //throw new QueryException(1);

            RoomType room_type  = EmuExtensions.ParseEnum<RoomType>(q.GetAttribute("room_type"));
            Mission mission     = Mission.GetMission(room_type, q.GetAttribute("mission"));
            string group_id     = q.GetAttribute("group_id");
            
            if (mission == null)
                throw new QueryException(1);
            
            if (!mission.Channels.Contains(client.Channel.ChannelType))
                throw new QueryException(1);

            if(room_type == RoomType.PvP_ClanWar && client.Profile.ClanId == 0)
                throw new QueryException(13);

            GameRoom room = GameRoom.CreateRoom(client, mission, room_type);

            var rCore           = room.GetExtension<GameRoomCore>();
            var rCustomParams   = room.GetExtension<GameRoomCustomParams>();

            rCustomParams.SetRestrictions(q);
            rCore.Private = q.GetAttribute("private") == "1";

            if(room_type == RoomType.PvP_ClanWar)
                rCore.CanPause = true;

            room.SetRoomName(q.GetAttribute("room_name"));

            room.JoinPlayer(client, group_id);

            XmlElement gameroom_open = Xml.Element("gameroom_open")
                      .Attr("room_id", room.Id)
                      .Attr("room_type", (int)room.Type);

            iq.SetQuery(gameroom_open.Child(room.Serialize(true)));
            client.QueryResult(iq);
        }
    }
}
