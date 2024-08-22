using EmuWarface.Core;
using EmuWarface.Game;
using MySql.Data.MySqlClient;
using System;
using System.Linq;

namespace EmuWarface.Commands
{
    public class BanCommand : ICmd
    {
        public Permission MinPermission => Permission.Moderator;
        public string Usage => "ban <nickname> [time]";
        public string Example => "ban user1 15d";
        public string[] Names => new[] { "ban" };

        public string OnCommand(Permission permission, string[] args)
        {
            if (args.Length == 0)
                return $"Invalid arguments.\nExample:\n{Example}";

            string nickname = args[0];
            string time = args.Length == 2 ? args[1] : "0s";

            try
            {
                var seconds = Utils.GetTotalSeconds(time);
                long unban_time = 0;

                if (seconds == -1)
                {
                    return $"Unknown time. 1d1h1m - 1 day 1 hour 1 minute";
                }
                if (seconds != 0)
                {
                    unban_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + seconds;
                }

                Profile profile = Profile.GetProfileForNickname(nickname);
                if (profile == null)
                {
                    return $"Player with nickname '{nickname}' not found.";
                }

                ulong user_id = Profile.GetUserId(profile.Id);

                var db = SQL.QueryRead($"SELECT * FROM emu_bans WHERE user_id={user_id}");
                if (db.Rows.Count != 0)
                {
                    long ban_time = (long)db.Rows[0]["unban_time"];
                    if (ban_time > DateTimeOffset.UtcNow.ToUnixTimeSeconds() || ban_time == 0)
                    {
                        return $"Player with nickname '{nickname}' has already been banned.";
                    }
                    else
                    {
                        //SQL.QueryRead($"DELETE FROM emu_bans WHERE user_id={user_id}");
                    }
                }

                try
                {
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_bans (`user_id`, `rule`, `unban_time`) VALUES (@user_id, @rule, @unban_time);");
                    cmd.Parameters.AddWithValue("@user_id", user_id);
                    cmd.Parameters.AddWithValue("@rule", "1");
                    cmd.Parameters.AddWithValue("@unban_time", unban_time);
                    SQL.QueryRead(cmd);

                }
                catch (Exception e)
                {
                    string exception = e.ToString();

                    if (exception.Contains("Duplicate"))
                    {
                        return $"Player with nickname '{nickname}' has already been banned.";
                    }
                    else
                    {
                        return $"Player could not be banned.";
                    }
                }

                lock (Server.Clients)
                {
                    Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nickname)?.Dispose();
                }
            }
            catch (ServerException e)
            {
                return e.Message;
            }
            return $"Player with nickname '{nickname}' is banned.";
        }
    }
}
