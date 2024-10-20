using EmuWarface.Core;
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
    public class Achievement
    {
        //<chunk achievement_id='51' progress='100000' completion_time='1489274661'>
        public ulong ProfileId      { get; private set; }
        public uint AchievementId    { get; private set; }
        public int Progress         { get; private set; }
        public long CompletionTimeUnixTimestamp { get; private set; }
        public bool IsCompleted => CompletionTimeUnixTimestamp != 0;

        public Achievement(ulong profile_id, uint achiev_id, int progress, long completionTime)
        {
            AchievementId   = achiev_id;
            ProfileId       = profile_id;
            Progress        = progress;
            CompletionTimeUnixTimestamp = completionTime;

            Insert();
        }

        Achievement()
        {

        }


        public static List<Achievement> GetAchievements(ulong profile_id)
        {
            var result = new List<Achievement>();

            var rows = SQL.QueryRead($"SELECT * FROM emu_achievements WHERE profile_id={profile_id}").Rows;

            foreach (DataRow row in rows)
            {
                result.Add(ParseDataRow(row));
            }

            return result;
        }

        public static Achievement SetAchiev(ulong profile_id, uint achiev_id, int progress, long completionTime)
        {
            var achiev = GetPlayerAchiev(profile_id, achiev_id);

            achiev.Progress = progress;
            achiev.CompletionTimeUnixTimestamp = completionTime;

            achiev.Update();

            return achiev;
        }

        public static Achievement GetPlayerAchiev(ulong profile_id, uint achiev_id)
        {
            List<Achievement> achievs = Profile.GetProfile(profile_id)?.Achievements;
            lock (achievs)
            {
                if (achievs == null)
                    throw new InvalidOperationException("Achievemenmts is not allowed be null");

                var achiev = achievs.FirstOrDefault(x => x.AchievementId == achiev_id);

                if (achiev != null)
                    return achiev;

                achiev = new Achievement(profile_id, achiev_id, 0, 0);
                achievs.Add(achiev);
                return achiev;
            }
        }

        public static List<Achievement> GetPlayerStats(ulong profile_id)
        {
            List<Achievement> achievs = new List<Achievement>();

            var res = SQL.QueryRead($"SELECT * FROM emu_achievements WHERE profile_id={profile_id}");

            foreach (DataRow row in res.Rows)
            {
                achievs.Add(ParseDataRow(row));
            }

            return achievs;
        }

        public void Insert()
        {
            if (ProfileId == 0)
                throw new InvalidOperationException();

            MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_achievements (profile_id, achievement_id, progress, completion_time) VALUES(@profile_id, @achievement_id, @progress, @completion_time) ON DUPLICATE KEY UPDATE progress=@progress;");

            cmd.Parameters.AddWithValue("@profile_id",      ProfileId);
            cmd.Parameters.AddWithValue("@achievement_id",  AchievementId);
            cmd.Parameters.AddWithValue("@progress",        Progress);
            cmd.Parameters.AddWithValue("@completion_time", CompletionTimeUnixTimestamp);

            SQL.Query(cmd);
        }

        public void Update()
        {
            if (ProfileId == 0)
                throw new InvalidOperationException();
            
            MySqlCommand cmd = new MySqlCommand("UPDATE emu_achievements SET progress=@progress, completion_time=@completion_time WHERE achievement_id=@achievement_id AND profile_id=@profile_id");

            cmd.Parameters.AddWithValue("@profile_id", ProfileId);
            cmd.Parameters.AddWithValue("@achievement_id", AchievementId);
            cmd.Parameters.AddWithValue("@progress", Progress);
            cmd.Parameters.AddWithValue("@completion_time", CompletionTimeUnixTimestamp);

            SQL.Query(cmd);
        }

        public static Achievement ParseDataRow(DataRow row)
        {
            return new Achievement
            {
                ProfileId                   = Convert.ToUInt64(row["profile_id"]),
                AchievementId               = (uint)row["achievement_id"],
                Progress                    = (int)row["progress"],
                CompletionTimeUnixTimestamp = (long)row["completion_time"],
            };
        }

        public XmlElement Serialize()
        {
            return Xml.Element("chunk")
                .Attr("achievement_id", AchievementId)
                .Attr("progress", Progress)
                .Attr("completion_time", CompletionTimeUnixTimestamp);
        }
    }
}
