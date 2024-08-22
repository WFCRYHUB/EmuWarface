
using EmuWarface.Core;
using EmuWarface.Game.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.Missions
{
    public class Mission
    {
        public string Uid               { get; private set; }
        public string Name              { get; private set; }
        public string TimeOfDay         { get; private set; }
        public string GameMode          { get; private set; }
        public string Channels          { get; private set; } = "pve";
        public string Difficulty        { get; private set; } = "normal";
        public string MissionType       { get; private set; } = "";
        public bool OnlyClanWarMission  { get; private set; }
        public bool ClanWarMission      { get; private set; }
        public bool ReleaseMission      { get; private set; }
        public XmlElement Element       { get; private set; }
        /*
         <mission name="@pvp_mission_display_name_stm_blackwood" time_of_day="20" game_mode="stm" game_mode_cfg="stm_new_mode.cfg"
        uid="13357a80-1c3f-49f9-be88-3f95ef931246" clan_war_mission="1" only_clan_war_mission="0"
        release_mission="1" channels="pvp_newbie, pvp_skilled, pvp_pro">
        <Basemap name="pvp/stm_blackwood"/>
            <UI>
                <Description text="@pvp_stm_mission_desc" icon="mapImgstm_blackwood"/>
                <GameMode text="@pvp_stm_game_mode_desc" icon="tdm_icon"/>
            </UI>
         <Sublevels mission_flow="default" win_pool="530" lose_pool="425" draw_pool="439" score_pool="100"/>
         <TimeDependency min_time="60" full_time="912" />
         <KillDependency min_kills="10" full_kills="20" />
         <Objectives />
         <Teleports />
        </mission>
         */

        public Mission(XmlElement mission)
        {
            Uid                 = mission.GetAttribute("uid");
            Name                = mission.GetAttribute("name");
            TimeOfDay           = mission.GetAttribute("time_of_day");
            GameMode            = mission.GetAttribute("game_mode");
            OnlyClanWarMission  = mission.GetAttribute("only_clan_war_mission") == "1" ? true : false;
            ClanWarMission      = mission.GetAttribute("clan_war_mission") == "1" ? true : false;
            ReleaseMission      = mission.GetAttribute("release_mission") == "1" ? true : false;

            if(GameMode == "pve")
            {
                Difficulty  = mission.GetAttribute("difficulty");
                MissionType = mission.GetAttribute("mission_type");
                Channels    = GameMode;
            }
            else
            {
                Channels = mission.GetAttribute("channels");
            }

            Element = mission;
        }

        public static Mission GetMission(RoomType type, string uid)
        {
            if(type == RoomType.PvE || type == RoomType.PvE_Autostart || type == RoomType.PvE_Private)
            {
                //throw new QueryException(1);
                return GameData.PvEMissions.FirstOrDefault(x => x.Uid == uid);
            }

            return GameData.PvPMissions.FirstOrDefault(x => x.Uid == uid);
        }

        public static Mission GetRandomMission(RoomType type, string mode = "")
        {
            Random rand = new Random();

            List<Mission> list;

            if(type == RoomType.PvP_Rating)
            {
                list = GameData.PvPMissions.Where(x => x.Element.GetAttribute("rating_game_mission") == "1" && x.ReleaseMission).ToList();
            }
            else if (!string.IsNullOrEmpty(mode))
            {
                if (type == RoomType.PvE_Autostart || type == RoomType.PvE_Private)
                {
                    list = GameData.PvEMissions.Where(x => x.GameMode == mode && x.ReleaseMission).ToList();
                }
                else
                {
                    list = GameData.QuickPlayPvP.Where(x => x.GameMode == mode && x.ReleaseMission).ToList();
                }
            }
            else
            {
                if (type == RoomType.PvE_Autostart || type == RoomType.PvE_Private)
                {
                    list = GameData.PvEMissions.Where(x => x.ReleaseMission).ToList();
                }
                else
                {
                    list = GameData.QuickPlayPvP.Where(x => x.GameMode != "pve" && x.ReleaseMission).ToList();
                }
            }

            return list[rand.Next(list.Count())];
        }

        public static List<Mission> GetMissionsVote(RoomType type, Mission currentMission)
        {
            Random rand = new Random();

            List<Mission> missions;

            switch(type)
            {
                case RoomType.PvE_Autostart:
                    missions = GameData.QuickPlayPvE;
                    break;
                case RoomType.PvP_Autostart:
                    missions = GameData.QuickPlayPvP;
                    break;
                default:
                    return null;
            }

            Mission map1 = currentMission;
            Mission map2;
            Mission map3;

            while (true)
            {
                map2 = missions[rand.Next(missions.Count())];
                map3 = missions[rand.Next(missions.Count())];

                if (map1 != map2 && map2 != map3 && map1 != map3)
                    break;
            }

            return new List<Mission> { map1, map2, map3 };
        }
    }
}
