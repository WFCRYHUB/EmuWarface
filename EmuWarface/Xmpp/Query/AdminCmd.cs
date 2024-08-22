using EmuWarface.Core;
using EmuWarface.Game.Notifications;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmuWarface.Xmpp.Query
{
    public static class AdminCmd
    {
        //<admin_cmd command='give' args='item 2222'/>

        [Query(IqType.Get, "admin_cmd")]
        public static void AdminCmdSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;
            var cmd = CommandHandler.Handlers.FirstOrDefault(c => c.Names.Contains(q.GetAttribute("command")));

            string result;
            if (cmd == null)
            {
                result = "Unknown command. Use '/remote help' for get command list.";
            }
            else if (cmd.MinPermission > client.Permission)
            {
                result = "You don't have permission.";
            }
            else
            {
                List<string> args = q.GetAttribute("args").Split(' ').ToList();
                args.RemoveAll(x => x == " " || x == string.Empty);

                result = cmd.OnCommand(client.Permission, args.ToArray());
            }

            q.Attr("result", result.Split("\n").FirstOrDefault());

            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace("<", "(").Replace(">", ")").Replace("\n", "<br>");
                Notification.SyncNotifications(client, Notification.MessageNotification($"EmuWarface v{string.Format("{0}.{1}.{2}", Server.Version.Major, Server.Version.Minor, Server.Version.Build)}<br>" + result));
            }
            //q.Attr("result", result.Replace("\n", "<br>"));
            client.QueryResult(iq);
        }
    }
}
