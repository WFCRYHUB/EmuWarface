using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Notifications;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuWarface.Commands
{
    public class MuteCommand : ICmd
    {
        public Permission MinPermission => Permission.Moderator;
        public string Usage => "mute <nickname> [time]";
        public string Example => "mute user1 15d";
        public string[] Names => new[] { "mute", "m" };

        public string OnCommand(Permission permission, string[] args)
        {
            if (args.Length == 0)
                return $"Invalid arguments.\nExample:\n{Example}";

            string nickname = args[0];
            string time = args.Length == 2 ? args[1] : "0s";

            var seconds = Utils.GetTotalSeconds(time);
            long unmute_time = 0;

            if (seconds == -1)
            {
                return $"Unknown time. 1d1h1m - 1 day 1 hour 1 minute";
            }
            if (seconds != 0)
            {
                unmute_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + seconds;
            }

            Profile profile = Profile.GetProfileForNickname(nickname);
            if (profile == null)
            {
                return $"Player with nickname '{nickname}' not found.";
            }

            ulong user_id = Profile.GetUserId(profile.Id);

            var db = SQL.QueryRead($"SELECT * FROM emu_mutes WHERE user_id={user_id}");
            if (db.Rows.Count != 0)
            {
                long mute_time = (long)db.Rows[0]["unmute_time"];
                if (mute_time > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return $"Player with nickname '{nickname}' has already been muted.";
                }
                else
                {
                    SQL.QueryRead($"DELETE FROM emu_mutes WHERE user_id={user_id}");
                }
            }

            try
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_mutes (`user_id`, `rule`, `unmute_time`) VALUES (@user_id, @rule, @unmute_time);");
                cmd.Parameters.AddWithValue("@user_id", user_id);
                cmd.Parameters.AddWithValue("@rule", "1");
                cmd.Parameters.AddWithValue("@unmute_time", unmute_time);
                SQL.QueryRead(cmd);

            }
            catch (Exception e)
            {
                string exception = e.ToString();

                if (exception.Contains("Duplicate"))
                {
                    return $"Player with nickname '{nickname}' has already been muted.";
                }
                else
                {
                    return $"Player could not be mute.";
                }
            }

            //кик
            Client client = null;
            lock (Server.Clients)
            {
                client = Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nickname);
            }

            if (client != null)
            {
                var notif = Notification.MessageNotification("Вы лишены возможности использовать чат до " + DateTimeOffset.FromUnixTimeSeconds(unmute_time‬).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                Notification.SyncNotifications(client, notif);
            }

            return $"Player with nickname '{nickname}' is muted.";
        }
    }
}
