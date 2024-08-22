using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class GameRoomQuickPlay
    {
        /*
<gameroom_quickplay 
        room_type="8" 
        room_name="Комната игрока settimeout" 
        mission_id="e5981b6a-325d-42eb-a3fe-e6eed0bc4bf2" 
        game_mode="" 
        status="0" 
        team_id="0" 
        class_id="4" 
        missions_hash="" 
        content_hash="" 
        timestamp="0" 
        uid="50f01404-87d1-4f19-9876-3e2900dc8114" />
         */

        [Query(IqType.Get, "gameroom_quickplay")]
        public static void GameRoomQuickPlaySerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            client.Profile.Room?.LeftPlayer(client);

            var q = iq.Query;

            //game_mode="tdm"
            //mission_id="e5981b6a-325d-42eb-a3fe-e6eed0bc4bf2" 

            var group = new List<Client>();
            foreach (XmlElement player in q.GetElementsByTagName("player"))
            {
                Client target = null;
                lock (Server.Clients)
                {
                    target = Server.Clients.FirstOrDefault(x => x.ProfileId.ToString() == player.GetAttribute("profile_id"));
                }

                if (target != null)
                    group.Add(target);
            }
            group.Add(client);

            var group_id = q.GetAttribute("uid");
            var game_mode = q.GetAttribute("game_mode");
            var mission_id = q.GetAttribute("mission_id");
            var room_type = Utils.ParseEnum<RoomType>(q.GetAttribute("room_type"));
            //Quickplay.Started(client);

            switch (room_type)
            {
                case RoomType.PvE_Autostart:
                case RoomType.PvP_Autostart:
                    {
                        GameRoom room = null;
                        lock (client.Channel.Rooms)
                        {
                            foreach (GameRoom r in client.Channel.Rooms)
                            {
                                if (r.Type != room_type)
                                    continue;

                                var rMission = r.GetExtension<GameRoomMission>(false);

                                if (!string.IsNullOrEmpty(game_mode) && rMission.Mission.GameMode != game_mode)
                                    continue;

                                if (!string.IsNullOrEmpty(mission_id) && rMission.Mission.Uid != mission_id)
                                    continue;

                                //TODO temp
                                if (r.GetExtension<GameRoomSession>().Status == SessionStatus.PostGame)
                                    continue;

                                var rCore = r.GetExtension<GameRoomCore>(false);
                                var rCustomParams = r.GetExtension<GameRoomCustomParams>(false);

                                if (rCore.Players.Count + group.Count > int.Parse(rCustomParams.GetCurrentRestriction("max_players")))
                                    continue;

                                room = r;
                            }
                        }

                        if (room == null)
                        {
                            //TODO если game_mode а не миссия
                            Mission mission = null;
                            if (!string.IsNullOrEmpty(mission_id))
                            {
                                mission = Mission.GetMission(room_type, mission_id);
                            }
                            if (!string.IsNullOrEmpty(game_mode))
                            {
                                mission = Mission.GetRandomMission(room_type, game_mode);
                            }

                            if (mission == null)
                            {
                                mission = Mission.GetRandomMission(room_type);
                            }

                            if (!mission.Channels.Contains(client.Channel.ChannelType))
                                throw new QueryException(1);

                            room = GameRoom.CreateRoom(client, mission, room_type);
                        }
                        foreach (var group_member in group)
                        {
                            group_member.Profile.RoomPlayer = new RoomPlayerInfo(room, group_id, RoomPlayerStatus.Ready);

                            Quickplay.Started(group_member);
                            Quickplay.RoomOffer(room, group_member);
                            Quickplay.Succeeded(group_member);
                        }
                    }
                    break;
                case RoomType.PvP_Rating:
                    {
                        int max_players = Config.GameRoom.PVP_RATING_MIN_PLAYERS_READY;

                        Notification.SyncNotifications(client, Notification.AnnouncementNotification("123\nfff"));

                        GameRoomCore rCore = null;
                        GameRoomCustomParams rCustomParams = null;

                        Team team = Team.None;

                        GameRoom room = null;
                        lock (client.Channel.Rooms)
                        {
                            foreach (GameRoom r in client.Channel.Rooms)
                            {
                                if (r.Type != room_type)
                                    continue;

                                if (r.GetExtension<GameRoomSession>().Status != SessionStatus.None)
                                    continue;

                                rCore = r.GetExtension<GameRoomCore>(false);
                                rCustomParams = r.GetExtension<GameRoomCustomParams>(false);

                                if (rCore.PlayersReserved.Count + group.Count > max_players)
                                    continue;

                                if (rCore.PlayersReserved.Count(x => x.Profile.RoomPlayer != null && x.Profile.RoomPlayer.TeamId == Team.Warface) + group.Count <= max_players / 2)
                                {
                                    team = Team.Warface;
                                }
                                if (rCore.PlayersReserved.Count(x => x.Profile.RoomPlayer.TeamId == Team.Blackwood) + group.Count <= max_players / 2)
                                {
                                    team = Team.Blackwood;
                                }

                                if (team != Team.None)
                                    room = r;
                            }
                        }

                        if (room == null)
                        {
                            Mission mission = Mission.GetRandomMission(room_type);

                            if (!mission.Channels.Contains(client.Channel.ChannelType))
                                throw new QueryException(1);

                            room = GameRoom.CreateRoom(client, mission, room_type);

                            rCore = room.GetExtension<GameRoomCore>(false);
                            rCustomParams = room.GetExtension<GameRoomCustomParams>(false);

                            team = Team.Warface;
                        }

                        rCustomParams.SetRestriction("max_players", max_players.ToString());

                        foreach (var group_member in group)
                        {
                            group_member.Profile.RoomPlayer = new RoomPlayerInfo(room, group_id, RoomPlayerStatus.Ready);
                            group_member.Profile.RoomPlayer.TeamId = team;

                            Quickplay.Started(group_member);

                            lock (rCore.PlayersReserved)
                            {
                                rCore.PlayersReserved.Add(group_member);

                                if (rCore.PlayersReserved.Count == max_players)
                                {
                                    foreach (var player in rCore.PlayersReserved)
                                    {
                                        Quickplay.RoomOffer(room, player);
                                        Quickplay.Succeeded(player);
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new QueryException(1);
            }

            iq.SetQuery(Xml.Element("gameroom_quickplay"));
            client.QueryResult(iq);
        }
    }
}
