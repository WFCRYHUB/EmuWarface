using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GameRoomUpdatePvP
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

        [Query(IqType.Get, "gameroom_update_pvp")]
        public static void GameRoomUpdatePvPSerializer(Client client, Iq iq)
        {
            //TODO проверять карту и канал на котором он создает
            if (client.Profile == null || client.Profile.Room == null)
                throw new QueryException(1);

            var q = iq.Query;

            var roomPlayer = client.Profile.RoomPlayer;
            var room = client.Profile.RoomPlayer.Room;

            var rCore           = room.GetExtension<GameRoomCore>();
            var rMaster         = room.GetExtension<GameRoomMaster>();
            var rMission        = room.GetExtension<GameRoomMission>();
            var rCustomParams   = room.GetExtension<GameRoomCustomParams>();

            if (rCore == null || rMaster == null || rMaster.Client != client)
                throw new QueryException(1);

            Mission mission = null;
            string qMission = q.GetAttribute("mission_key");
            if (qMission != rMission.Key)
            {
                mission = Mission.GetMission(room.Type, qMission);

                if(mission == null)
                    throw new QueryException(1);

                if (!mission.Channels.Contains(client.Channel.ChannelType))
                    throw new QueryException(1);
            }

            if (room.Type != RoomType.PvP_Public && room.Type != RoomType.PvP_ClanWar)
                throw new QueryException(2);

            if (mission != null)
                room.SetMission(mission);


            //room.SetRoomName(q.GetAttribute("room_name"));

            rCore.Private = q.GetAttribute("private") == "1" ? true : false;

            rCustomParams.SetRestrictions(q);

            room.Update();

            XmlElement gameroom_update_pvp = Xml.Element("gameroom_update_pvp").Child(room.Serialize(true));

            iq.SetQuery(gameroom_update_pvp);
            client.QueryResult(iq);
        }
    }
}
