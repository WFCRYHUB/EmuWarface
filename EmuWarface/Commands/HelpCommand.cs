using EmuWarface.Core;
using EmuWarface.Game.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EmuWarface.Commands
{
    public class HelpCommand : ICmd
    {
        public Permission MinPermission => Permission.None;
        public string Usage => "help";
        public string Example => "help";
        public string[] Names => new[] { "help" };

        public string OnCommand(Permission permission, string[] args)
        {
            return "Command list:\n" + string.Join('\n', CommandHandler.Handlers.Where(c => permission >= c.MinPermission).Select(c => c.Usage).ToArray());
        }
    }
}
