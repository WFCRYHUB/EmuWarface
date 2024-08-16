using EmuWarface.Game.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace EmuWarface
{
    public static class EmuConfig
    {
        public static SettingsConfig Settings; 
        public static SqlConfig Sql; 
        public static GameRoomConfig GameRoom; 
        public static List<MasterServerConfig> MasterServers;
        public static List<DefaultItemConfig> DefaultItems;
        public static List<string> ObsceneWords;

        static EmuConfig()
        {
            //API             = LoadConfig<APIConfig>("Config/api.json");
            Settings        = LoadConfig<SettingsConfig>("Config/settings.json");
            Sql             = LoadConfig<SqlConfig>("Config/sql.json");
            //Market          = LoadConfig<MarketConfig>("Config/market.json");
            GameRoom        = LoadConfig<GameRoomConfig>("Config/room.json");
            MasterServers   = LoadConfig<List<MasterServerConfig>>("Config/masterservers.json");
            //TODO сломалось создание профиля
            DefaultItems    = LoadConfig<List<DefaultItemConfig>>("Config/defaultItems.json");
            ObsceneWords    = LoadConfig<List<string>>("Config/obsceneWords.json");
        }

        public static T LoadConfig<T>(string fileName)
        {
            if (!File.Exists(fileName))
            {
                File.CreateText(fileName).Dispose();
                throw new FileNotFoundException();
            }

            using (StreamReader reader = File.OpenText(fileName))
            {
                var text = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(text);
            }
        }
    }

    public class MasterServerConfig
    {
        [JsonProperty("resource")]
        public string Resource { get; set; }
        [JsonProperty("server_id")]
        public int ServerId { get; set; }
        [JsonProperty("channel")]
        public string Channel { get; set; }
        [JsonProperty("rank_group")]
        public string RankGroup { get; set; }
        [JsonProperty("min_rank")]
        public int MinRank { get; set; }
        [JsonProperty("max_rank")]
        public int MaxRank { get; set; }
        [JsonProperty("bootstrap")]
        public string Bootstrap { get; set; }
    }

    public class SqlConfig
    {
        [JsonProperty("server")]
        public string Server { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("database")]
        public string Database { get; set; }
        [JsonProperty("characterSet")]
        public string CharacterSet { get; set; }
        [JsonProperty("port")]
        public uint Port { get; set; }
    }

    public class DefaultItemConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public ItemSlot Type { get; set; }
        [JsonProperty("classes")]
        public Class Classes { get; set; }
    }

    public class SettingsConfig
    {
        [JsonProperty("host")]
        public string Host { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
        [JsonProperty("rconPort")]
        public int RconPort { get; set; }
        [JsonProperty("rconAllowedHosts")]
        public List<string> RconHosts { get; set; }
        [JsonProperty("dedicatedHosts")]
        public List<string> DedicatedHosts { get; set; }
        [JsonProperty("onlinePath")]
        public string OnlinePath { get; set; }
        [JsonProperty("gameVersion")]
        public string GameVersion { get; set; }
        [JsonProperty("certSecret")]
        public string CertSecret { get; set; }
        [JsonProperty("xmpp_debug")]
        public bool XmppDebug { get; set; }
        [JsonProperty("xmpp_debug_console")]
        public bool XmppDebugConsole { get; set; }
        [JsonProperty("use_online_protect")]
        public bool UseOnlineProtect { get; set; }
    }

    public class GameRoomConfig
    {
        /*
         public const int ROOM_PVP_PUBLIC_MIN_PLAYERS_READY = 2;
        public const int ROOM_PVP_AUTOSTART_MIN_PLAYERS_READY = 2;
        public const int ROOM_PVE_PRIVATE_MIN_PLAYERS_READY = 1;
        public const int ROOM_PVE_AUTOSTART_MIN_PLAYERS_READY = 2;
        public const int ROOM_PVP_CLANWAR_MIN_PLAYERS_READY = 4;
         */
        [JsonProperty("min_players_ready_pvp_public")]
        public int PVP_PUBLIC_MIN_PLAYERS_READY { get; set; }
        [JsonProperty("min_players_ready_pvp_autostart")]
        public int PVP_AUTOSTART_MIN_PLAYERS_READY { get; set; }
        [JsonProperty("min_players_ready_pve_private")]
        public int PVE_PRIVATE_MIN_PLAYERS_READY { get; set; }
        [JsonProperty("min_players_ready_pve_autostart")]
        public int PVE_AUTOSTART_MIN_PLAYERS_READY { get; set; }
        [JsonProperty("min_players_ready_pvp_clanwar")]
        public int PVP_CLANWAR_MIN_PLAYERS_READY { get; set; }
        [JsonProperty("min_players_ready_pvp_rating")]
        public int PVP_RATING_MIN_PLAYERS_READY { get; set; }
    }
}
