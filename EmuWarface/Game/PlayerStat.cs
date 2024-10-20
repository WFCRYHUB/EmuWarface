using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public class PlayerStat
    {
        public ulong Id             { get; private set; }
        public string Stat          { get; private set; }
        public ulong Value          { get; private set; }
        public string Difficulty    { get; private set; }
        public PlayMode? Mode       { get; private set; }
        public Class? Class         { get; private set; }
        public string ItemType      { get; private set; }
        public ulong ProfileId      { get; private set; }

        public PlayerStat(string stat, ulong value, ulong profileId, string difficulty = null, PlayMode? mode = null, Class? @class = null, string itemType = null)
        {
            Stat = stat;
            Value = value;
            Difficulty = difficulty;
            Mode = mode;
            Class = @class;
            ItemType = itemType;
            ProfileId = profileId;

            Insert();
        }

        PlayerStat()
        {

        }

        public static XmlElement GetPlayerStats(List<PlayerStat> stats)
        {
            XmlElement get_player_stats = Xml.Element("get_player_stats");

            stats.ForEach(stat =>
            {
                if (stat.Stat != "player_wpn_usage")
                    get_player_stats.Child(stat.Serialize());
            });

            foreach(var @class in (Class[])Enum.GetValues(typeof(Class)))
            {
                get_player_stats.Child(GetMaxWeaponUsage(stats, @class)?.Serialize());
            }

            return get_player_stats;
        }

        public static PlayerStat GetMaxWeaponUsage(List<PlayerStat> stats, Class @class)
        {
            PlayerStat selected = null;
            ulong time = 0;

            foreach(var stat in stats)
            {
                if(stat.Stat == "player_wpn_usage" && stat.Class == @class && stat.Value > time)
                {
                    selected    = stat;
                    time        = stat.Value;
                }
            }

            return selected;
        }

        public static void IncrementPlayerStat(ulong profile_id, string name, ulong value, string difficulty = null, PlayMode? mode = null, Class? @class = null, string itemType = null)
        {
            var stat = GetPlayerStat(profile_id, name, difficulty, mode, @class, itemType);

            stat.Value += value;

            stat.Update();
        }

        public static void SetPlayerStat(ulong profile_id, string name, ulong value, string difficulty = null, PlayMode? mode = null, Class? @class = null, string itemType = null)
        {
            var stat = GetPlayerStat(profile_id, name, difficulty, mode, @class, itemType);

            stat.Value = value;

            stat.Update();
        }

        public static PlayerStat GetPlayerStat(ulong profile_id, string name, string difficulty = null, PlayMode? mode = null, Class? @class = null, string itemType = null)
        {
            //List<PlayerStat> stats = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id)?.Profile?.Stats;
            List<PlayerStat> stats = Profile.GetProfile(profile_id)?.Stats;

            if (stats == null)
                throw new InvalidOperationException("PlayerStats is not allowed be null");

            var stat = stats.FirstOrDefault(x => x.Stat == name && x.Difficulty == difficulty && x.Mode == mode && x.Class == @class && x.ItemType == itemType);

            if (stat != null)
                return stat;

            stat = new PlayerStat(name, 0, profile_id, difficulty, mode, @class, itemType);
            stats.Add(stat);

            return stat;
        }

        public static List<PlayerStat> GetPlayerStats(ulong profile_id)
        {
            List<PlayerStat> stats = new List<PlayerStat>();

            var res = SQL.QueryRead($"SELECT * FROM emu_stats WHERE profile_id={profile_id}");

            foreach (DataRow row in res.Rows)
            {
                stats.Add(ParseDataRow(row));
            }

            return stats;
        }
        public void Insert()
        {
            if (ProfileId == 0)
                throw new InvalidOperationException();

            MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_stats (profile_id, stat, class, mode, difficulty, item_type, value) VALUES(@profile_id, @stat, @class, @mode, @difficulty, @item_type, @value) ON DUPLICATE KEY UPDATE value=@value; SELECT LAST_INSERT_ID();");
            
            cmd.Parameters.AddWithValue("@profile_id", ProfileId);
            cmd.Parameters.AddWithValue("@stat", Stat);
            cmd.Parameters.AddWithValue("@value", Value);
            cmd.Parameters.AddWithValue("@class", Class != null ? (object)(byte)Class : DBNull.Value);
            cmd.Parameters.AddWithValue("@mode", Mode != null ? (object)(byte)Mode : DBNull.Value);
            cmd.Parameters.AddWithValue("@difficulty", Difficulty != null ? (object)Difficulty : DBNull.Value);
            cmd.Parameters.AddWithValue("@item_type", ItemType != null ? (object)ItemType : DBNull.Value);

            Id = Convert.ToUInt64(SQL.QueryRead(cmd).Rows[0][0]);
        }

        public void Update()
        {
            if (ProfileId == 0)
                throw new InvalidOperationException();

            //MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_stats (profile_id, stat, class, mode, difficulty, item_type, value) VALUES(@profile_id, @stat, @class, @mode, @difficulty, @item_type, @value) ON DUPLICATE KEY UPDATE value=@value");
            MySqlCommand cmd = new MySqlCommand("UPDATE emu_stats SET class=@class, mode=@mode, difficulty=@difficulty, item_type=@item_type, value=@value WHERE id=@id");

            cmd.Parameters.AddWithValue("@id",          Id);
            cmd.Parameters.AddWithValue("@value",       Value);
            cmd.Parameters.AddWithValue("@class",       Class != null ? (object)(byte)Class : DBNull.Value);
            cmd.Parameters.AddWithValue("@mode",        Mode != null ? (object)(byte)Mode : DBNull.Value);
            cmd.Parameters.AddWithValue("@difficulty",  Difficulty != null ? (object)Difficulty : DBNull.Value);
            cmd.Parameters.AddWithValue("@item_type",   ItemType != null ? (object)ItemType : DBNull.Value);

            SQL.Query(cmd);
        }

        private static PlayerStat ParseDataRow(DataRow row)
        {
            return new PlayerStat
            {
                Id          = Convert.ToUInt64(row["id"]),
                ProfileId   = Convert.ToUInt64(row["profile_id"]),
                Value       = Convert.ToUInt64(row["value"]),
                Stat        = (string)row["stat"],
                Difficulty  = row["difficulty"] != DBNull.Value ? (string)row["difficulty"] : null,
                ItemType    = row["item_type"] != DBNull.Value ? (string)row["item_type"] : null,
                Mode        = row["mode"] != DBNull.Value ? (PlayMode?)(byte)row["mode"] : null,
                Class       = row["class"] != DBNull.Value ? (Class?)(byte)row["class"] : null
            };
        }

        //TODO test не тестил все виды
        public XmlElement Serialize()
        {
            XmlElement stat = Xml.Element("stat")
                .Attr("stat", Stat)
                .Attr("Value", Value);

            if (Mode != null)
                stat.Attr("mode", Mode);

            if (Class != null)
                stat.Attr("class", Class);

            if (!string.IsNullOrEmpty(Difficulty))
                stat.Attr("difficulty", Difficulty);

            if (!string.IsNullOrEmpty(ItemType))
                stat.Attr("item_type", ItemType);

            return stat;
        }
    }
}
