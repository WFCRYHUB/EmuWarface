using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public static class StatsManager
    {
        public static void CalculateSessionStats(XmlElement telemetry)
        {
            XmlElement sessionStats = telemetry["stats_session"];
            XmlElement players      = telemetry["players"];

            XmlElement timelines    = sessionStats["timelines"];

            //TODO
            if (string.IsNullOrEmpty(sessionStats.GetAttribute("gamemode")))
                return;

            var difficulty  = sessionStats.GetAttribute("mission_type");
            var mode        = EmuExtensions.ParseEnum<PlayMode>(sessionStats.GetAttribute("gamemode"));

            Dictionary<ulong, ulong> disconnections     = GetDisconnection(timelines, mode);
            Dictionary<ulong, int> scores               = new Dictionary<ulong, int>();

            foreach (XmlElement player in players.ChildNodes)
            {
                if (player.Name != "player")
                    continue;

                var profile_id      = ulong.Parse(player.GetAttribute("profile_id"));
                var lifetime_begin  = ulong.Parse(player.GetAttribute("lifetime_begin"));
                var lifetime_end    = ulong.Parse(player.GetAttribute("lifetime_end"));

                //Пропуск если текущие данные были до последнего disconnect игрока
                if (disconnections.ContainsKey(profile_id) && disconnections[profile_id] > lifetime_begin)
                    continue;

                if (sessionStats.GetAttribute("winner") != "0" && sessionStats.GetAttribute("winner") != "-1")
                {
                    if (player.GetAttribute("team") == sessionStats.GetAttribute("winner"))
                    {
                        scores[profile_id] = 1;
                    }
                    else
                    {
                        scores[profile_id] = 2;
                    }
                }
                else
                {
                    scores[profile_id] = 0;
                }

                XmlElement playerTimelines = player["timelines"];

                var @class = EmuExtensions.ParseEnum<Class>(player.GetAttribute("character_class"));

                CalculateResurrections(playerTimelines,
                    profile_id);

                CalculateWeaponsUsage(playerTimelines, lifetime_end,
                    profile_id, mode, @class);

                CalculateShots(playerTimelines, 
                    profile_id, mode, @class);

                CalculateHits(playerTimelines, 
                    profile_id, mode, @class);

                CalculateDeaths(playerTimelines, 
                    profile_id, mode);

                CalculateKills(playerTimelines, 
                    profile_id, mode);

                CalculateScore(playerTimelines, 
                    profile_id, @class);

                CalculateClimbs(playerTimelines,
                    profile_id);

                CalculateClimbsAssist(playerTimelines,
                    profile_id);

                //Подсчёт времени игры за текущий класс
                ulong play_time = (lifetime_end - lifetime_begin) / 100;
                PlayerStat.IncrementPlayerStat(profile_id, "player_playtime", play_time, @class: @class, mode: mode);
            }

            foreach(var player in scores)
            {
                var profile_id = player.Key;

                CalculateMatchResult(sessionStats, 
                    profile_id, player.Value, mode, difficulty);

                CalculateMaxSessionTime(sessionStats, 
                    profile_id);

                //TODO liked weapon
            }
        }

        private static void CalculateMatchResult(XmlElement sessionStats, ulong profile_id, int winStatus, PlayMode mode, string mission_type)
        {
            string name;

            switch (winStatus)
            {
                case 1:
                    name = "player_sessions_won";
                    break;
                case 2:
                    name = "player_sessions_lost";
                    break;
                default:
                    //0 и -1
                    name = "player_sessions_draw";
                    break;
            }

            PlayerStat.IncrementPlayerStat(profile_id, name, value: 1, difficulty: mission_type, mode: mode);
        }

        private static void CalculateMaxSessionTime(XmlElement sessionStats, ulong profile_id)
        {
            var sessionTime = ulong.Parse(sessionStats.GetAttribute("session_time"));
            var statMaxSessionTime = PlayerStat.GetPlayerStat(profile_id, "player_max_session_time");

            if (sessionTime * 10 > statMaxSessionTime.Value)
                PlayerStat.SetPlayerStat(profile_id, "player_max_session_time", sessionTime * 10);
        }

        private static void CalculateWeaponsUsage(XmlElement playerTimelines, ulong lifetime_end, ulong profile_id, PlayMode mode, Class @class)
        {
            //TODO
            //TIME IS ULONG

            XmlElement timeline = GetTimeline(playerTimelines, "weapon");

            if (timeline == null)
                return;

            string lastWeaponName = string.Empty;
            ulong lastWeaponTime = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                if (lastWeaponTime != 0)
                    PlayerStat.IncrementPlayerStat(profile_id, "player_wpn_usage", value: (ulong.Parse(val.GetAttribute("time")) - lastWeaponTime) / 1000, @class: @class, itemType: val.GetAttribute("prm"));

                lastWeaponName = val.GetAttribute("prm");
                lastWeaponTime = ulong.Parse(val.GetAttribute("time"));
            }

            if (lastWeaponTime != 0)
                PlayerStat.IncrementPlayerStat(profile_id, "player_wpn_usage", value: lifetime_end - lastWeaponTime / 1000, @class: @class, itemType: lastWeaponName);
        }

        private static void CalculateKills(XmlElement playerTimelines, ulong profile_id, PlayMode mode)
        {
            XmlElement timeline = GetTimeline(playerTimelines, "kill");

            if (timeline == null)
                return;

            ulong ai = 0;
            ulong friendly = 0;
            ulong player = 0;
            ulong claymore = 0;
            ulong melee = 0;
            ulong streak = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                var param = val["param"];

                if (param.GetAttribute("friendly_fire") == "1")
                {
                    friendly++;
                    continue;
                }

                streak++;

                switch (param.GetAttribute("hit_type"))
                {
                    case "claymore":
                        {
                            claymore++;
                        }
                        break;
                    case "melee":
                    case "melee_secondary":
                        {
                            melee++;
                        }
                        break;
                }

                if (param.GetAttribute("is_player") == "1")
                {
                    player++;
                }
                else
                {
                    ai++;
                }
            }

            var max_streak = PlayerStat.GetPlayerStat(profile_id, "player_kill_streak", mode: mode).Value;

            if (streak > max_streak)
                PlayerStat.SetPlayerStat(profile_id, "player_kill_streak", streak, mode: mode);

            PlayerStat.IncrementPlayerStat(profile_id, "player_kills_ai", ai, mode: mode);
            PlayerStat.IncrementPlayerStat(profile_id, "player_kills_player", player, mode: mode);
            PlayerStat.IncrementPlayerStat(profile_id, "player_kills_melee", melee, mode: mode);
            PlayerStat.IncrementPlayerStat(profile_id, "player_kills_claymore", claymore, mode: mode);
            PlayerStat.IncrementPlayerStat(profile_id, "player_kills_player_friendly", friendly, mode: mode);
        }

        private static void CalculateDeaths(XmlElement playerTimelines, ulong profile_id, PlayMode mode)
        {
            XmlElement timeline = GetTimeline(playerTimelines, "death");

            if (timeline == null)
                return;

            ulong deaths = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                deaths++;
            }

            PlayerStat.IncrementPlayerStat(profile_id, "player_deaths", deaths, mode: mode);
        }

        private static void CalculateShots(XmlElement playerTimelines, ulong profile_id, PlayMode mode, Class @class)
        {
            XmlElement timeline = GetTimeline(playerTimelines, "shot");

            if (timeline == null)
                return;

            ulong shots = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                shots++;
            }

            PlayerStat.IncrementPlayerStat(profile_id, "player_shots", shots, mode: mode, @class: @class);
        }

        private static void CalculateHits(XmlElement playerTimelines, ulong profile_id, PlayMode mode, Class @class)
        {
            XmlElement timeline = GetTimeline(playerTimelines, "hit");

            if (timeline == null)
                return;

            ulong max_damage = PlayerStat.GetPlayerStat(profile_id, "player_max_damage").Value;
            ulong hits = 0;
            ulong all_damage = 0;
            ulong headshots = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                var param = val["param"];

                var damage = ulong.Parse(param.GetAttribute("damage"));
                switch (param.GetAttribute("hit_type"))
                {
                    case "healing":
                        //TODO проверить точно ли damage
                        PlayerStat.IncrementPlayerStat(profile_id, "player_heal", damage);
                        break;
                    case "repair":
                        //TODO проверить точно ли damage
                        PlayerStat.IncrementPlayerStat(profile_id, "player_repair", damage);
                        break;
                    default:
                        if(damage > max_damage)
                            max_damage = damage;

                        all_damage += damage;

                        if(param.GetAttribute("fatal") == "1" && param.GetAttribute("material_type") == "head")
                            headshots++;

                        hits++;
                        break;
                }
            }

            PlayerStat.SetPlayerStat(profile_id, "player_max_damage", max_damage);
            PlayerStat.IncrementPlayerStat(profile_id, "player_damage", all_damage);
            PlayerStat.IncrementPlayerStat(profile_id, "player_hits", hits, mode: mode, @class: @class);
            PlayerStat.IncrementPlayerStat(profile_id, "player_headshots", headshots, mode: mode, @class: @class);
        }

        private static void CalculateResurrections(XmlElement playerTimeLines, ulong profile_id)
        {
            XmlElement timeline = GetTimeline(playerTimeLines, "resurrect");

            if (timeline == null)
                return;

            ulong defibrillator = 0;
            ulong coin = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                switch (val.GetAttribute("prm"))
                {
                    case "defibrillator":
                        //PlayerStat.IncrementPlayerStat(profile_id, "player_resurrected_by_medic", 1, @class: @class);
                        //PlayerStat.IncrementPlayerStat(profile_id, "player_resurrect_made", 1, @class: @class);
                        defibrillator++;
                        break;
                    case "coin":
                        //PlayerStat.IncrementPlayerStat(profile_id, "player_resurrected_by_coin", 1);
                        coin++;
                        break;
                }
            }

            PlayerStat.IncrementPlayerStat(profile_id, "player_resurrected_by_medic", defibrillator);
            PlayerStat.IncrementPlayerStat(profile_id, "player_resurrected_by_coin", coin);
        }

        private static void CalculateClimbs(XmlElement playerTimeLines, ulong profile_id)
        {
            XmlElement timeline = GetTimeline(playerTimeLines, "climb_coop");

            if (timeline == null)
                return;

            ulong climbs = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                if (val.GetAttribute("prm") == "end")
                    climbs++;
            }

            PlayerStat.IncrementPlayerStat(profile_id, "player_climb_coops", climbs);
        }

        private static void CalculateClimbsAssist(XmlElement playerTimeLines, ulong profile_id)
        {
            XmlElement timeline = GetTimeline(playerTimeLines, "climb_assist");

            if (timeline == null)
                return;

            ulong climb_assists = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                if (val.GetAttribute("prm") == "end")
                    climb_assists++;
            }

            PlayerStat.IncrementPlayerStat(profile_id, "player_climb_assists", climb_assists);
        }

        private static void CalculateScore(XmlElement playerTimeLines, ulong profile_id, Class @class)
        {
            XmlElement timeline = GetTimeline(playerTimeLines, "score");

            if (timeline == null)
                return;

            ulong resurrect = 0;
            ulong give_ammo = 0;

            foreach (XmlElement val in timeline.ChildNodes)
            {
                if (val.Name != "val")
                    continue;

                var param = val["param"];

                switch (param.GetAttribute("event"))
                {
                    case "teammate_resurrect":
                        resurrect++;
                        break;
                    case "teammate_give_ammo":
                        give_ammo++;
                        break;
                    //OLD
                    /*case "sm_coop_climb":
                        PlayerStat.IncrementPlayerStat(profile_id, "player_climb_coops", 1);
                        break;
                    case "sm_coop_assist":
                        PlayerStat.IncrementPlayerStat(profile_id, "player_climb_assists", 1);
                        break;*/
                }
            }
            PlayerStat.IncrementPlayerStat(profile_id, "player_resurrect_made", resurrect, @class: @class);
            PlayerStat.IncrementPlayerStat(profile_id, "player_ammo_restored", give_ammo);
        }

        private static Dictionary<ulong, ulong> GetDisconnection(XmlElement timelines, PlayMode mode)
        {
            Dictionary<ulong, ulong> list = new Dictionary<ulong, ulong>();

            var timeline = GetTimeline(timelines, "disconnect");
            if (timeline != null)
            {
                foreach (XmlElement val in timeline.ChildNodes)
                {
                    if (val.Name != "val")
                        continue;

                    var param = val["param"];
                    var profile_id = ulong.Parse(param.GetAttribute("profile_id"));

                    if (param.GetAttribute("cause") == "session_ended")
                        continue;

                    list[profile_id] = ulong.Parse(val.GetAttribute("time"));

                    switch (param.GetAttribute("cause"))
                    {
                        case "kicked":
                            PlayerStat.IncrementPlayerStat(profile_id, "player_sessions_kicked", 1, mode: mode);
                            break;
                        case "left":
                            PlayerStat.IncrementPlayerStat(profile_id, "player_sessions_left", 1, mode: mode);
                            break;
                        default:
                            PlayerStat.IncrementPlayerStat(profile_id, "player_sessions_lost_connection", 1, mode: mode);
                            break;
                    }
                }
            }
            return list;
        }

        private static XmlElement GetTimeline(XmlElement timelines, string name)
        {
            foreach (XmlElement timeline in timelines.ChildNodes)
            {
                if (timeline.HasAttributes && timeline.GetAttribute("name") == name)
                {
                    return timeline;
                }
            }

            return null;
        }
    }
}
