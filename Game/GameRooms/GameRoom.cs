using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRoomVotes;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
    public partial class GameRoom : IDisposable
    {
        public const string ROOM_DEFAULT_NAME = "Комната игрока {0}";
        public const string QUICK_ROOM_DEFAULT_NAME = "Быстрая игра #{0}";
        /*
#if DEBUGLOCAL
        public const int ROOM_PVP_AUTOSTART_MIN_PLAYERS_READY = 1;
        public const int ROOM_PVE_AUTOSTART_MIN_PLAYERS_READY = 1;
        public const int ROOM_PVP_PUBLIC_MIN_PLAYERS_READY = 1;
        public const int ROOM_PVP_CLANWAR_MIN_PLAYERS_READY = 1;
        public const int ROOM_PVE_PRIVATE_MIN_PLAYERS_READY = 1;
#else
        public const int ROOM_PVP_PUBLIC_MIN_PLAYERS_READY = 2;
        public const int ROOM_PVP_AUTOSTART_MIN_PLAYERS_READY = 2;
        public const int ROOM_PVE_PRIVATE_MIN_PLAYERS_READY = 1;
        public const int ROOM_PVE_AUTOSTART_MIN_PLAYERS_READY = 2;
        public const int ROOM_PVP_CLANWAR_MIN_PLAYERS_READY = 4;
#endif
        */
        public bool Disposed;

        private static long _seedId;
        private List<GameRoomExtension> _extensions = new List<GameRoomExtension>();

        public long Id              { get; }
        public RoomType Type        { get; private set; }
        public MasterServer Channel { get; private set; }
        public bool IsPrivate => GetExtension<GameRoomCore>().Private;
        public bool RoomMasterExpire;
        public bool ModOnlyPrimary;

        public T GetExtension<T>(bool update = true)
        {
            if (Disposed)
                throw new ServerException("Room deleted");

            GameRoomExtension extension = _extensions.FirstOrDefault(x => x is T);

            if (update)
                extension?.Update();

            return (T)(object)extension;
        }


        public GameRoom(MasterServer channel, RoomType type)
        {
            Id = ++_seedId;
            Channel = channel;
            Type = type;

            //TODO maybe отключать таймер при удалении комнаты
            var timer = new Timer(600)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += Sync;
        }

        public static GameRoom CreateRoom(Client initiator, Mission mission/*, XmlElement q*/, RoomType type/*, string group_id*/)
        {
            GameRoom room = new GameRoom(initiator.Channel, type);

            room._extensions.Add(new GameRoomSession());
            room._extensions.Add(new GameRoomMission(mission));
            room._extensions.Add(new GameRoomVoteStates());
            room._extensions.Add(new GameRoomInGameChat(room));
            //TODO на рм кикать нельзя

            var rCustomParams = new GameRoomCustomParams(room);
            rCustomParams.SetDefaultRestrictions();

            room._extensions.Add(rCustomParams);

            switch (type)
            {
                case RoomType.PvP_Public:
                    {
                        room._extensions.Add(new GameRoomCore(string.Format(ROOM_DEFAULT_NAME, initiator.Profile.Nickname), EmuConfig.GameRoom.PVP_PUBLIC_MIN_PLAYERS_READY));
                        room._extensions.Add(new GameRoomMaster(initiator));
                    }
                    break;
                case RoomType.PvE_Private:
                    {
                        room._extensions.Add(new GameRoomCore(string.Format(ROOM_DEFAULT_NAME, initiator.Profile.Nickname), EmuConfig.GameRoom.PVE_PRIVATE_MIN_PLAYERS_READY));
                        room._extensions.Add(new GameRoomMaster(initiator));
                    }
                    break;
                case RoomType.PvP_Autostart:
                    {
                        room._extensions.Add(new GameRoomCore(string.Format(QUICK_ROOM_DEFAULT_NAME, room.Id), EmuConfig.GameRoom.PVP_AUTOSTART_MIN_PLAYERS_READY));
                        room._extensions.Add(new GameRoomAutoStart(room));
                        room._extensions.Add(new GameRoomSquadsColors(room));
                    }
                    break;
                case RoomType.PvE_Autostart:
                    {
                        room._extensions.Add(new GameRoomCore(string.Format(ROOM_DEFAULT_NAME, initiator.Profile.Nickname), EmuConfig.GameRoom.PVE_AUTOSTART_MIN_PLAYERS_READY));
                        room._extensions.Add(new GameRoomAutoStart(room));
                        room._extensions.Add(new GameRoomSquadsColors(room));
                    }
                    break;
                case RoomType.PvP_ClanWar:
                    {
                        room._extensions.Add(new GameRoomCore(string.Format(ROOM_DEFAULT_NAME, initiator.Profile.Nickname), EmuConfig.GameRoom.PVP_CLANWAR_MIN_PLAYERS_READY));
                        room._extensions.Add(new GameRoomClanWar(Clan.GetClanName(initiator.Profile.ClanId)));
                        room._extensions.Add(new GameRoomMaster(initiator));
                    }
                    break;
                case RoomType.PvP_Rating:
                    {
                        room._extensions.Add(new GameRoomCore(string.Format(ROOM_DEFAULT_NAME, initiator.Profile.Nickname), EmuConfig.GameRoom.PVP_RATING_MIN_PLAYERS_READY));
                        room._extensions.Add(new GameRoomAutoStart(room));
                        room._extensions.Add(new GameRoomSquadsColors(room));
                    }
                    break;
                default:
                    throw new QueryException(1);
            }

            lock (initiator.Channel.Rooms)
            {
                initiator.Channel.Rooms.Add(room);
            }
            //initiator.Channel.Rooms.Add(room);

            return room;
        }

        public void ReservePlayer(Client client, string group_id)
        {
            var rCore = GetExtension<GameRoomCore>();

            client.Profile.RoomPlayer = new RoomPlayerInfo(this, group_id, RoomPlayerStatus.Ready);

            lock (rCore.PlayersReserved)
            {
                rCore.PlayersReserved.Add(client);
            }

            Update();
        }

        public void JoinPlayer(Client client, string groupId/*, RoomPlayerStatus status*/)
        {
            if (Disposed)
            {
                client.Profile.RoomPlayer = null;
                Channel.Rooms.RemoveAll(x => x.Id == Id);

                Log.Warn("[GameRoom] Room deleted, extensions null (room_id: {0})", Id);
                return;
            }

            var rCore = GetExtension<GameRoomCore>();

            RoomPlayerStatus status = RoomPlayerStatus.NotReady;
            Team teamId = Team.Warface;

            switch (Type)
            {
                case RoomType.PvP_Public:
                    {
                        //AutoPlayerBalance(rCore, client);
                        if (rCore.PlayersWarfaceCount > rCore.PlayersBlackwoodCount)
                            teamId = Team.Blackwood;
                    }
                    break;
                case RoomType.PvE_Autostart:
                case RoomType.PvP_Autostart:
                case RoomType.PvP_Rating:
                    {
                        status = RoomPlayerStatus.Ready;
                    }
                    break;
                case RoomType.PvP_ClanWar:
                    {
                        var rClanWar        = GetExtension<GameRoomClanWar>();
                        var rCustomParams   = GetExtension<GameRoomCustomParams>();
                        var clan = client.Profile.ClanName;

                        var max_players = int.Parse(rCustomParams.GetCurrentRestriction("max_players"));

                        if (rCore.Players.Count >= max_players)
                            throw new QueryException(4);

                        if (string.IsNullOrEmpty(rClanWar.ClanFirst) || rClanWar.ClanFirst == clan)
                        {
                            if (rCore.PlayersWarfaceCount >= (max_players / 2))
                                throw new QueryException(4);

                            rClanWar.ClanFirst = clan;

                            teamId = Team.Warface;
                            break;
                        }
                        if (string.IsNullOrEmpty(rClanWar.ClanSecond) || rClanWar.ClanSecond == clan)
                        {
                            if (rCore.PlayersBlackwoodCount >= (max_players / 2))
                                throw new QueryException(4);

                            rClanWar.ClanSecond = clan;

                            teamId = Team.Blackwood;
                            break;
                        }

                        throw new QueryException(14);
                    }
                    break;
                default:
                    {
                        groupId = string.Empty;
                    }
                    break;
            }

            if (Type != RoomType.PvP_Rating)
            {
                client.Profile.RoomPlayer = new RoomPlayerInfo(this, groupId, status);
                client.Profile.RoomPlayer.TeamId = teamId;
            }

            lock (rCore.PlayersReserved)
            {
                rCore.PlayersReserved.Remove(client);
            }
            lock (rCore.Players)
            {
                rCore.Players.Add(client);
            }
            //rCore.Players.Add(client);
            GetExtension<GameRoomSquadsColors>()?.UpdateSquad(groupId);

            if (rCore.MinReadyPlayers <= rCore.Players.Count)
            {
                var rAutoStart = GetExtension<GameRoomAutoStart>();

                if (rAutoStart != null && !rAutoStart.AutoStartTimeout)
                    rAutoStart.Start();
            }

            Update();
        }

        public void LeftPlayer(Client client, RoomPlayerRemoveReason reason = RoomPlayerRemoveReason.Left)
        {
            if (Disposed)
            {
                client.Profile.RoomPlayer = null;

                Log.Warn("[GameRoom] Room deleted, extensions null (room_id: {0})", Id);
                return;
            }

            var rCore = GetExtension<GameRoomCore>();
            var rSession = GetExtension<GameRoomSession>();
            var rMaster = GetExtension<GameRoomMaster>();

            lock (rCore.Players)
            {
                rCore.Players.Remove(client);
            }
            //rCore.Players.Remove(client);
            lock (rCore.PlayersReserved)
            {
                rCore.PlayersReserved.Remove(client);
            }

            if (rCore.Players.Count == 0 && rCore.PlayersReserved.Count == 0)
            {
                Dispose();

                return;
            }

            GetExtension<GameRoomSquadsColors>()?.UpdateSquad(client.Profile.RoomPlayer.GroupId);

            client.Profile.RoomPlayer = null;

            lock (rCore.LeftPlayers)
            {
                if (reason != RoomPlayerRemoveReason.Left && reason != RoomPlayerRemoveReason.KickTimeout)
                {
                    if (rCore.LeftPlayers.ContainsKey(client.ProfileId))
                        rCore.LeftPlayers[client.ProfileId] = reason;
                    else
                        rCore.LeftPlayers.Add(client.ProfileId, reason);
                }
            }

            if (rMaster?.Client == client)
                SetMaster(rCore.Players.First());

            if (Type == RoomType.PvP_Rating && rCore.PlayersReserved.Count == 0 && rSession.Status == SessionStatus.None)
            {
                foreach (var player in rCore.Players.ToList())
                {
                    KickPlayer(player, RoomPlayerRemoveReason.KickRankedGameCouldnotStart);
                }
            }
            else
            {
                Update();
            }
        }

        public void KickPlayer(Client client, RoomPlayerRemoveReason reason)
        {
            client.QueryGet(Xml.Element("gameroom_on_kicked").Attr("reason", (int)reason));

            LeftPlayer(client, reason);
        }

        public void SetMaster(Client client)
        {
            var rCore   = GetExtension<GameRoomCore>();
            var rMaster = GetExtension<GameRoomMaster>();

            if (!client.Presence.HasFlag(PlayerStatus.InGame))
            {
                client.Profile.RoomPlayer.Status = RoomPlayerStatus.NotReady;
                rCore.Update();
            }

            rMaster.Set(client);
        }

        public void Sync(object source, ElapsedEventArgs e) => Sync();
        public void Sync()
        {
            //TODO delete room
            if (Disposed || _extensions == null)
                return;

            XmlElement game_room = Serialize();

            lock (_extensions)
            {
                foreach (var ext in _extensions)
                {
                    if (ext.Check())
                        game_room.Child(ext.Serialize());
                }
            }

            if (game_room.ChildNodes.Count == 0)
                return;

            var rCore = GetExtension<GameRoomCore>(false);
            var rSession = GetExtension<GameRoomSession>(false);
            var rMission = GetExtension<GameRoomMission>(false);

            if (rCore.Players.Count == 0)
            {
                //TODO удалить таймер
                //Dispose();
                return;
            }

            XmlElement gameroom_sync = Xml.Element("gameroom_sync")
                .Child(game_room);

            Iq iq = new Iq(IqType.Get, "none", "k01.warface");
            lock (rCore.Players)
            {
                rCore.Players.ForEach(roomPlayer =>
                {
                    iq.To = roomPlayer.Jid;
                    roomPlayer.Send(iq.SetQuery(gameroom_sync));
                });
            }

            MissionUpdate(rCore, rSession, game_room);
        }
        public void MissionUpdate(GameRoomCore rCore, GameRoomSession rSession, XmlElement game_room)
        {
            //game_room["mission"].Attr("data", EmuExtensions.Base64Encode(rMission.Mission.Element.OuterXml));

            if (rSession.Dedicated != null)
                rSession.Dedicated.Client.QueryGet(Xml.Element("mission_update").Child(game_room), Channel.Jid);
        }

        public void Update()
        {
            //AutoTeamBalance();
            //ValidateClasses();

            switch (Type)
            {
                case RoomType.PvE_Private:
                case RoomType.PvE_Autostart:
                    ValidateMissionAccess();
                    //AutoTeamBalance();
                    break;
                case RoomType.PvP_Autostart:
                    AutoTeamBalance();
                    break;
                case RoomType.PvP_ClanWar:
                    ValidateClasses();
                    ValidateClanWar();
                    break;
                case RoomType.PvP_Rating:
                    break;
                default:
                    AutoTeamBalance();
                    ValidateClasses();
                    break;
            }
        }

        private void ValidateClanWar()
        {
            var rClanWar = GetExtension<GameRoomClanWar>();
            var rCore = GetExtension<GameRoomCore>();

            if (rCore.PlayersWarfaceCount == 0)
                rClanWar.ClanFirst = string.Empty;

            if (rCore.PlayersBlackwoodCount == 0)
                rClanWar.ClanSecond = string.Empty;

            List<Client> toKick = new List<Client>();

            lock (rCore.Players)
            {
                foreach (var target in rCore.Players)
                {
                    var clanId = target.Profile.ClanId;
                    if (clanId == 0)
                    {
                        toKick.Add(target);
                        continue;
                    }

                    var clan = target.Profile.ClanName;
                    if (rClanWar.ClanFirst == clan && target.Profile.RoomPlayer.TeamId != Team.Warface)
                    {
                        toKick.Add(target);
                        continue;
                    }
                    else if (rClanWar.ClanSecond == clan && target.Profile.RoomPlayer.TeamId != Team.Blackwood)
                    {
                        toKick.Add(target);
                        continue;
                    }
                }
            }

            toKick.ForEach(x => KickPlayer(x, RoomPlayerRemoveReason.KickClan));
        }

        private void AutoTeamBalance()
        {
            var rCore           = GetExtension<GameRoomCore>();
            var rCustomParams   = GetExtension<GameRoomCustomParams>();

            //int max_players = int.Parse(rCustomParams.GetCurrentRestriction("max_players"));

            if (rCustomParams.GetCurrentRestriction("auto_team_balance") == "1")
            {
                Team currentTeam = Team.Warface;
                lock (rCore.Players)
                {
                    foreach (var target in rCore.Players)
                    {
                        target.Profile.RoomPlayer.TeamId = currentTeam;
                        currentTeam = currentTeam == Team.Warface ? Team.Blackwood : Team.Warface;
                    }
                }
                /*foreach (var target in rCore.Players)
                {
                    target.Profile.RoomPlayer.TeamId = currentTeam;
                    currentTeam = currentTeam == Team.Warface ? Team.Blackwood : Team.Warface;
                }*/
            }
            /*if (Type == RoomType.PvP_Autostart || Type == RoomType.PvP_Public)
            {
                Team currentTeam;

                lock (rCore.Players)
                {
                    foreach (var target in rCore.Players)
                    {
                        if (rCore.PlayersWarface.Count > rCore.PlayersBlackwood.Count)
                        {
                            currentTeam = Team.Blackwood;
                        }
                        else
                        {
                            currentTeam = Team.Warface;
                        }

                        target.Profile.RoomPlayer.TeamId = currentTeam;
                    }
                }
            }*/
        }

        public void StartRatingTimer()
        {
            if (Type != RoomType.PvP_Rating)
                return;

            var rCore       = GetExtension<GameRoomCore>();
            var rSession    = GetExtension<GameRoomSession>();

            EmuExtensions.Delay(5).ContinueWith(task =>
            {
                if (rCore.MinReadyPlayers != rCore.Players.Count)
                {
                    foreach (var player in rCore.Players.ToList())
                    {
                        KickPlayer(player, RoomPlayerRemoveReason.KickRankedGameCouldnotStart);
                    }
                }
                else
                {
                    rSession.Status = SessionStatus.InGame;
                    rSession.Update();
                }
            });
        }

        public void StartMasterLoseTimer(Client master)
        {
            if (Type == RoomType.PvE_Autostart || Type == RoomType.PvP_Autostart || Type == RoomType.PvP_Rating)
                return;

            if (RoomMasterExpire)
                return;

            var rCore       = GetExtension<GameRoomCore>();
            var rSession    = GetExtension<GameRoomSession>();
            var rMaster     = GetExtension<GameRoomMaster>();

            if (rMaster.Client != master)
                return;

            if (!rMaster.Client.Presence.HasFlag(PlayerStatus.InShop) && !rMaster.Client.Presence.HasFlag(PlayerStatus.InCustomize) && !rMaster.Client.Presence.HasFlag(PlayerStatus.Away))
                return;

            if (rCore.Players.Count == 1)
                return;

            if (rCore.MinReadyPlayers > rCore.Players.Count)
                return;

            if (rCore.Players.Count(x => x.Profile.RoomPlayer.Status == RoomPlayerStatus.Ready) != rCore.Players.Count - 1)
                return;

            if (rSession.Status != SessionStatus.None)
                return;

            XmlElement gameroom_loosemaster = Xml.Element("gameroom_loosemaster")
                .Attr("time", "10");

            rMaster.Client.QueryGet(gameroom_loosemaster);

            RoomMasterExpire = true;

            EmuExtensions.Delay(10).ContinueWith(task => 
            {
                EndMasterLoseTimer(rMaster.Client);
                RoomMasterExpire = false;
            });
        }

        public void EndMasterLoseTimer(Client master)
        {
            if (!RoomMasterExpire)
                return;

            var rCore       = GetExtension<GameRoomCore>();
            var rSession    = GetExtension<GameRoomSession>();
            var rMaster     = GetExtension<GameRoomMaster>();

            if (rMaster.Client != master)
                return;

            if (!rMaster.Client.Presence.HasFlag(PlayerStatus.InShop) && !rMaster.Client.Presence.HasFlag(PlayerStatus.InCustomize) && !rMaster.Client.Presence.HasFlag(PlayerStatus.Away))
                return;

            if (rCore.Players.Count == 1)
                return;

            if (rCore.MinReadyPlayers > rCore.Players.Count)
                return;

            if (rSession.Status != SessionStatus.None)
                return;

            if (rCore.Players.Count(x => x.Profile.RoomPlayer.Status == RoomPlayerStatus.Ready) != rCore.Players.Count - 1)
                return;

            var new_master = rCore.Players.FirstOrDefault(x => x != master);

            if (new_master == null)
                return;

            SetMaster(new_master);
        }

        public void ValidateClasses()
        {
            var rCore           = GetExtension<GameRoomCore>();
            var rCustomParams   = GetExtension<GameRoomCustomParams>();

            lock (rCore.Players)
            {
                foreach (Client client in rCore.Players)
                {
                    var classRestriction = rCustomParams.ClassRestriction;

                    if ((client.Profile.CurrentClass == ClassId.Rifleman && classRestriction.HasFlag(Class.Rifleman)) ||
                            (client.Profile.CurrentClass == ClassId.Heavy && classRestriction.HasFlag(Class.Heavy)) ||
                            (client.Profile.CurrentClass == ClassId.Medic && classRestriction.HasFlag(Class.Medic)) ||
                            (client.Profile.CurrentClass == ClassId.Engineer && classRestriction.HasFlag(Class.Engineer)) ||
                            (client.Profile.CurrentClass == ClassId.Recon && classRestriction.HasFlag(Class.Recon)))
                    {
                        if (!classRestriction.HasFlag(Class.Rifleman))
                            client.Profile.CurrentClass = ClassId.Rifleman;

                        if (!classRestriction.HasFlag(Class.Medic))
                            client.Profile.CurrentClass = ClassId.Medic;

                        if (!classRestriction.HasFlag(Class.Engineer))
                            client.Profile.CurrentClass = ClassId.Engineer;

                        if (!classRestriction.HasFlag(Class.Recon))
                            client.Profile.CurrentClass = ClassId.Recon;

                        if (!classRestriction.HasFlag(Class.Heavy))
                            client.Profile.CurrentClass = ClassId.Heavy;
                    }
                }
            }
        }

        public void SetMission(Mission mission)
        {
            var rMission    = GetExtension<GameRoomMission>();

            ValidateMissionAccess();

            rMission.Set(mission);
        }

        public void SetRoomName(string room_name)
        {
            var rCore = GetExtension<GameRoomCore>();

            if (GameData.ValidateInputString("RoomName", room_name) && room_name.Length > 0 && room_name.Length <= 32)
            {
                rCore.Name = room_name;

                //mods

                ModOnlyPrimary = room_name.Contains("МОД(1)");
            }
            else
            {
                throw new QueryException(1);
            }
        }

        public void ValidateMissionAccess()
        {
            if (Type != RoomType.PvE_Autostart && Type != RoomType.PvE_Private)
                return;

            var rCore = GetExtension<GameRoomCore>();

            lock (rCore.Players)
            {
                foreach (var client_not_exists_access in rCore.Players.Where(p => p.Profile.Items.Count(i => i.Name == "mission_access_token_04" && i.Quantity == 0) != 0))
                {
                    client_not_exists_access.Profile.RoomPlayer.Status = RoomPlayerStatus.Restricted;
                }
            }
        }

        public Client GetReadyDedicated()
        {
            lock (Server.Dedicateds)
            {
                foreach (var dedicated in Server.Dedicateds)
                {
                    if (dedicated.Dedicated.Status == SessionStatus.Ready && dedicated.Dedicated.Channel?.ChannelType == Channel.ChannelType)
                        return dedicated;
                }
            }
            /*foreach (var dedicated in Server.Dedicateds)
            {
                if (dedicated.Dedicated.Status == SessionStatus.Ready && dedicated.Dedicated.Channel?.ChannelType == Channel.ChannelType)
                    return dedicated;
            }*/

            return null;
        }

        public void MissionLoad()
        {
            var dedicated = GetReadyDedicated();
            if (dedicated != null)
            {
                MissionLoad(dedicated);
                return;
            }

            EmuExtensions.Delay(2).ContinueWith(task => MissionLoad());
        }

        public void MissionLoad(Client dedic)
        {
            dedic.Dedicated.Room = this;

            var rSession = GetExtension<GameRoomSession>();
            var rMission = GetExtension<GameRoomMission>();

            rSession.Dedicated = dedic.Dedicated;
            rSession.StartSession();

            XmlElement mission_load = Xml.Element("mission_load")
                .Attr("bootstrap_mode",     "0")
                .Attr("bootstrap_name",     "russia")
                .Attr("verbosity_level",    "1")
                .Attr("session_id", rSession.Id);

            XmlElement game_room = Serialize();
            lock (_extensions)
            {
                foreach (var ext in _extensions)
                {
                    game_room.Child(ext.Serialize());
                }
            }

            game_room["mission"].Attr("data", Convert.ToBase64String(Encoding.UTF8.GetBytes(rMission.Mission.Element.OuterXml)));

            XmlElement online_variables = Xml.Element("online_variables")
                .Child(GameData.SvVariables);

            var variable_item = Xml.Element("item");

            if (rMission.Mode == "pve")
            {
                variable_item.Attr("value", "extra");
            }
            else
            {
                variable_item.Attr("value", "extra");
            }

            variable_item.Attr("key", "cvar:sv_boost_rates");
            online_variables["variables"].Child(variable_item);

            variable_item.Attr("key", "cvar:cl_boost_rates");
            online_variables["variables"].Child(variable_item);

            //Log.Info("[OnlineVariables] " + online_variables.OuterXml);

            mission_load.Child(game_room);
            mission_load.Child(online_variables);
            mission_load.Child(GameData.AntiCheatConfiguration);

            Log.Info("[GameRoom] Mission load sent (jid: {0})", dedic.Jid);

            dedic.Dedicated.Client.QueryGet(mission_load, Channel.Jid);
        }

        public void StartMissionVoting()
        {
            if (Type != RoomType.PvE_Autostart && Type != RoomType.PvP_Autostart)
                return;
                
            var rCore       = GetExtension<GameRoomCore>();
            var rMission    = GetExtension<GameRoomMission>();
            var rVote       = GetExtension<GameRoomVoteStates>(false);

            rVote.MissionVote = new MissionVote(rCore.Players, new List<Mission>(Mission.GetMissionsVote(Type, rMission.Mission)));
        }

        public void EndMissionVoting()
        {
            var rVote           = GetExtension<GameRoomVoteStates>(false);
            var rMission        = GetExtension<GameRoomMission>();
            var rCustomParams   = GetExtension<GameRoomCustomParams>();

            var missionVote = rVote.MissionVote;

            if (rVote.MissionVote == null)
                return;

            missionVote.Timeout = true;
            Mission selected = null;
            int votes_num = 0;

            foreach (var mission in missionVote.Missions)
            {
                var num = missionVote.Votes.Count(x => x.Value == mission);

                if (num >= votes_num)
                {
                    votes_num = num;
                    selected = mission;
                }
            }

            SetMission(selected);
            rCustomParams.SetDefaultRestrictions();

            var on_voting_finished = Xml.Element("map_vote_finished")
                .Attr("mission_uid", selected.Uid);

            foreach (var voter in rVote.MissionVote.Voters)
            {
                voter.QueryGet(on_voting_finished, voter.Channel.Jid);
            }

            rVote.MissionVote = null;
        }

        public void EndSession() 
        {
            var rSession = GetExtension<GameRoomSession>();

            Log.Info("[GameRoom] EndSesion (room_id: {0}, session_id: {1})", Id, rSession.Id);

            rSession.Dedicated?.MissionUnload();
            rSession.EndSession();

            if (Type == RoomType.PvP_Autostart || Type == RoomType.PvE_Autostart)
            {
                GetExtension<GameRoomAutoStart>()?.EndGame();
            }
        }

        public void AbortSession()
        {
            var rCore       = GetExtension<GameRoomCore>();

            lock (rCore.Players)
            {
                foreach (var player in rCore.Players)
                {
                    player.Profile.RoomPlayer.Status = RoomPlayerStatus.NotReady;
                }
            }

            EndSession();
        }

        public void ShopSyncConsumables(ulong profile_id, XmlElement item)
        {
            var rSession = GetExtension<GameRoomSession>();

            if (rSession.Dedicated == null)
                return;

            var shop_sync_consumables = Xml.Element("shop_sync_consumables")
                .Attr("session_id", rSession.Id);

            shop_sync_consumables.Child(Xml.Element("profile_items")
                .Attr("profile_id", profile_id).Child(item));

            //Iq iq = new Iq(IqType.Get, rSession.Dedicated.Client.Jid, "k01.warface");
            rSession.Dedicated.Client.QueryGet(shop_sync_consumables);
        }

        public XmlElement Serialize(bool childExtensions = false)
        {
            var game_room = Xml.Element("game_room")
                .Attr("room_id", Id)
                .Attr("room_type", (int)Type);

            if (childExtensions)
            {
                lock (_extensions)
                {
                    _extensions.ForEach(ext => game_room.Child(ext.Serialize()));
                }
            }

            return game_room;

        }

        public void Dispose()
        {
            if (Disposed)
                return;

            var rCore = GetExtension<GameRoomCore>();

            GetExtension<GameRoomSession>()?.Dedicated?.MissionUnload();
            lock (rCore.Players)
            {
                rCore?.Players?.ForEach(x => x.Profile.RoomPlayer = null);
            }
            //rCore?.Players?.ForEach(x => x.Profile.RoomPlayer = null);

            lock (Channel.Rooms)
            {
                Channel.Rooms.Remove(this);
            }
            //Channel.Rooms.Remove(this);

            _extensions = null;

            Disposed = true;
        }
    }
}
