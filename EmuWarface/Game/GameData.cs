using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public static class GameData
    {
        private static Dictionary<int, int> _ranks;

        public static XmlElement MissionsList = Xml.Element("missions_get_list");

        public static List<Mission> PvEMissions = new List<Mission>();
        public static List<Mission> PvPMissions = new List<Mission>();

        public static List<Mission> QuickPlayPvP = new List<Mission>();
        public static List<Mission> QuickPlayPvE = new List<Mission>();

        public static XmlElement RandomBoxCards { get; private set; }
        public static XmlElement ClVariables    { get; private set; }
        public static XmlElement SvVariables    { get; private set; }
        public static XmlElement AntiCheatConfiguration { get; private set; }
        public static XmlElement GameModesConfig                { get; private set; }
        public static XmlElement RewardsConfiguration           { get; private set; }
        public static Dictionary<string, List<char>> CharacterMap   { get; private set; }
        public static Dictionary<string, XmlElement> GameModes  { get; private set; }

        public static void Init()
        {
            try
            {
                Load();
                LoadCharacterMap();
                LoadExperience();
                LoadCVars();
                LoadMissions();
                LoadQuickPlayMissions();
                LoadGameModes();
            }
            catch(FileNotFoundException e)
            {
                Log.Error("[GameData] File '{0}' not found", e.FileName);
                throw e;
            }
            catch (Exception e)
            {
                Log.Error("[GameData] Load failed");
                throw e;
            }
        }

        public static void Load()
        {
            RewardsConfiguration    = Xml.Load(GameDataConfig.REWARDS_CONFIGURATION);
            AntiCheatConfiguration  = Xml.Load(GameDataConfig.ANTICHEAT_CONFIG);
            RandomBoxCards          = Xml.Load(GameDataConfig.RANDOM_BOX_CARDS);
        }

        public static void LoadCharacterMap(string language = "Russian")
        {
            var map = Xml.Load(GameDataConfig.CHAR_MAP_CONFIGURATION);

            foreach(XmlElement lang in map.ChildNodes)
            {
                if (lang.GetAttribute("name") == language)
                {
                    CharacterMap = new Dictionary<string, List<char>>();

                    foreach (XmlElement preset in lang.ChildNodes)
                    {
                        var name = preset.GetAttribute("name");

                        Log.Info("[GameData] {0} preset '{1}' was loaded", lang.GetAttribute("name"), name);

                        CharacterMap.Add(name, new List<char>());

                        foreach (XmlElement range in preset.ChildNodes)
                        {
                            var start   = Convert.ToChar(Convert.ToUInt32(range.GetAttribute("start").Substring(2), 16));
                            var end     = Convert.ToChar(Convert.ToUInt32(range.GetAttribute("end").Substring(2), 16));

                            for (char i = start; i <= end; i++)
                            {
                                CharacterMap[preset.GetAttribute("name")].Add(i);
                                //Console.Write(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(new char[1] { i })));
                            }
                        }
                    }
                    break;
                }
            }
        }

        public static void LoadExperience()
        {
            XmlElement exp_curve = Xml.Load(GameDataConfig.EXP_CURVE_CONFIG);

            _ranks = new Dictionary<int, int>();

            foreach (XmlElement level in exp_curve.ChildNodes)
            {
                int rank = int.Parse(level.Name.Replace("level", ""));
                int exp = int.Parse(level.GetAttribute("exp"));

                _ranks[rank] = exp;
            }

            Log.Info("[GameData] Loaded {0} ranks", _ranks.Count);
        }

        public static void LoadCVars()
        {
            ClVariables = Xml.Load(GameDataConfig.CL_VARS_CONFIG);
            SvVariables = Xml.Load(GameDataConfig.SV_VARS_CONFIG);
            /*
                _cl_vars
                _sv_vars
            */

            Log.Info("[GameData] Loaded {0} cvars", ClVariables.ChildNodes.Count);
        }

        public static void LoadMissions()
        {
            foreach (var file in Directory.GetFiles(GameDataConfig.MISSIONS_CONFIG_FOLDER, "*.xml", SearchOption.AllDirectories))
            {
                PvPMissions.Add(new Mission(Xml.Load(file)));
            }

            //TODO temp

            foreach (var file in Directory.GetFiles(GameDataConfig.MISSIONS_CONFIG_PVE_FOLDER, "*.xml", SearchOption.AllDirectories))
            {
                PvEMissions.Add(new Mission(Xml.Load(file)));
            }

            LoadPvEMissions();

            Log.Info("[GameData] Loaded {0} PvE missions", PvEMissions.Count);
            Log.Info("[GameData] Loaded {0} PvP missions", PvPMissions.Count);
        }

        public static void LoadQuickPlayMissions()
        {
            foreach (XmlElement map in QueryCache.GetCache("quickplay_maplist").Data.ChildNodes)
            {
                var mission_key = map.GetAttribute("mission");
                var mission = PvPMissions.First(x => x.Uid == mission_key);

                QuickPlayPvP.Add(mission);
            }

            foreach (var mission in PvEMissions)
            {
                //TODO
                //easymission,normalmission,hardmission,zombieeasy,zombienormal,zombiehard,survivalmission,campaignsections,campaignsection1,campaignsection2,campaignsection3,volcanoeasy,volcanonormal,volcanohard,volcanosurvival,anubiseasy,anubisnormal,anubishard,anubiseasy2,anubisnormal2,anubishard2,zombietowereasy,zombietowernormal,zombietowerhard,icebreakereasy,icebreakernormal,icebreakerhard,chernobyleasy,chernobylnormal,chernobylhard,japaneasy,japannormal,japanhard,marseasy,marsnormal,marshard,blackwood,pve_arena
                //if (mission.MissionType == "easymission" || mission.MissionType == "easymission" || mission.MissionType == "easymission" || mission.MissionType == "easymission")
                    QuickPlayPvE.Add(mission);
            }

            Log.Info("[GameData] Loaded {0} quickplay PvE missions", QuickPlayPvE.Count);
            Log.Info("[GameData] Loaded {0} quickplay PvP missions", QuickPlayPvP.Count);
        } 

        public static void LoadPvEMissions()
        {
            foreach(var mission in PvEMissions)
            {
                //MissionsList.Child

                var node = new GameRoomMission(mission).Serialize();
                node.RemoveAttribute("revision");

                //TODO CrownRewardsThresholds
                node.Child(mission.Element.GetElementsByTagName("Objectives")[0]);
                node.Child(mission.Element.GetElementsByTagName("CrownRewardsThresholds")[0]);
                foreach(XmlElement reward in RewardsConfiguration["CrownRewards"].ChildNodes)
                {
                    if (reward.GetAttribute("type") == mission.MissionType)
                        node.Child(Xml.Element("CrownRewards")
                            .Attr("bronze", reward.GetAttribute("bronze"))
                            .Attr("silver", reward.GetAttribute("silver"))
                            .Attr("gold",   reward.GetAttribute("gold")));
                }

                MissionsList.Child(node);

  //<mission mission_key="a05c0322-1799-42d9-b11f-c0d9491392b9" no_teams="1" name="@na_mission_volcano_01" setting="survival/africa_survival_base" mode="pve" mode_name="@PvE_game_mode_desc" mode_icon="pve_icon" description="@na_mission_volcano_desc_01" image="mapImgNAvolcano_e" difficulty="easy" type="volcanoeasy" time_of_day="9:06">
  //  <objectives factor="1">
  //    <objective id="0" type="primary" />
  //  </objectives>
  //  <CrownRewardsThresholds>
  //    <TotalPerformance bronze="1130300" silver="1358000" gold="1520000" />
  //    <Time bronze="4190944" silver="4191784" gold="4192204" />
  //  </CrownRewardsThresholds>
  //  <CrownRewards bronze="6" silver="17" gold="32" />
  //</mission>
            }
        }

        public static void LoadGameModes()
        {
            GameModesConfig = Xml.Load(GameDataConfig.CONFIG_FNAME);
            GameModes       = new Dictionary<string, XmlElement>();

            foreach (var mode_file in Directory.GetFiles(GameDataConfig.CONFIG_GAME_MODES_FOLDER, "*.xml", SearchOption.AllDirectories))
            {
                var xmlMode = Xml.Load(mode_file);

                GameModes[xmlMode.GetAttribute("mode")] = xmlMode;
            }

            Log.Info("[GameData] Loaded {0} modes", GameModes.Count);
        }

        public static int GetRank(int experience)
        {
            if (_ranks == null || _ranks.Count == 0)
                throw new InvalidOperationException();

            int rank = 1;

            foreach (var level in _ranks)
            {
                if (experience < level.Value)
                    return rank;

                rank = level.Key;
            }

            return rank;
        }

        //Nickname Clanname Clandesc ChatText RoomName 
        public static bool ValidateInputString(string name, string value)
        {
#if DEBUG
            return true;
#endif

            //if (!EmuExtensions.MatFilter(value))
            //    return false;

            List<char> preset;

            if (!CharacterMap.TryGetValue(name, out preset))
                return false;

            foreach (char character in value) 
            { 
                if(!preset.Contains(character))
                    return false;
            }

            return true;
        }
    }
}
