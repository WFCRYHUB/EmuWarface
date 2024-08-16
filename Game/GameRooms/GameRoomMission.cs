using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
    public class GameRoomMission : GameRoomExtension
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Setting { get; set; }
        public string Mode { get; set; }
        public string ModeName { get; set; }
        public string ModeIcon { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Difficulty { get; set; }
        public string Type { get; set; }
        public string TimeOfDay { get; set; }
        public Mission Mission { get; set; }

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

        public GameRoomMission(Mission mission)
        {
            Set(mission);
        }

        public void Set(Mission mission)
        {
            Key             = mission.Uid;
            Name            = mission.Name;
            Setting         = mission.Element["Basemap"].GetAttribute("name");
            Mode            = mission.GameMode;
            ModeName        = mission.Element["UI"]["GameMode"].GetAttribute("text");
            ModeIcon        = mission.Element["UI"]["GameMode"].GetAttribute("icon");
            Description     = mission.Element["UI"]["Description"].GetAttribute("text");
            Image           = mission.Element["UI"]["Description"].GetAttribute("icon");
            Difficulty      = mission.Difficulty;
            Type            = mission.MissionType;
            TimeOfDay       = mission.TimeOfDay;

            Mission         = mission;
        }

        public override XmlElement Serialize()
        {
            /*<mission mission_key='cd54d2eb-f00e-4ccc-bbd4-d4c0f2cc935e' no_teams='0' name='@pvp_mission_display_name_stm_wharf' 
             * setting='pvp/stm_wharf' mode='stm' mode_name='@pvp_stm_game_mode_desc' mode_icon='tdm_icon' 
             * description='@pvp_stm_mission_desc' image='mapImgStmWharf' difficulty='normal' type='' time_of_day='09.30' revision='668'>*/

            if(Mission.Channels == "pve")
            {
                foreach (XmlElement mission in GameData.MissionsList.ChildNodes)
                {
                    if(mission.GetAttribute("mission_key") == Key)
                        return mission.Attr("revision", Revision);
                }
            }

            return Xml.Element("mission")
                .Attr("mission_key",    Key)
                .Attr("no_teams",       (Mode == "ffa" || Mode == "hnt") ? 1 : 0)
                .Attr("name",           Name)
                .Attr("mode",           Mode)
                .Attr("mode_name",      ModeName)
                .Attr("mode_icon",      ModeIcon)
                .Attr("description",    Description)
                .Attr("image",          Image)
                .Attr("difficulty",     Difficulty)
                .Attr("setting",        Setting)
                .Attr("type",           Type)
                .Attr("time_of_day",    TimeOfDay)
                .Attr("revision",       Revision);
        }
    }
}
