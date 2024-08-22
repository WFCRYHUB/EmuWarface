using EmuWarface.Core;
using EmuWarface.Game;
using MySql.Data.MySqlClient;
using System.Linq;
using System;
using EmuWarface.Game.Notifications;

namespace EmuWarface.Commands
{
    public class BroadcastCommand : ICmd
    {
        public Permission MinPermission => Permission.Moderator;
        public string Usage => "bc [text]";
        public string Example => "bc Hello world!";
        public string[] Names => new[] { "broadcast", "bc" };

        public string OnCommand(Permission permission, string[] args)
        {
            if (args.Length == 0)
                return $"Invalid arguments.\nExample:\n{Example}";

            var message = string.Join(' ', args);

            lock (Server.Clients)
            {
                foreach (var target in Server.Clients)
                {
                    Notification.SyncNotifications(target, Notification.AnnouncementNotification(message));
                }
            }

            return string.Empty;
        }
    }
}
