using EmuWarface.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuWarface.Commands
{
    public class OnlineCommand : ICmd
    {
        public Permission MinPermission => Permission.None;
        public string Usage => "online";
        public string Example => "online";
        public string[] Names => new[] { "online" };

        public string OnCommand(Permission permission, string[] args)
        {
            return "Online: " + Server.Clients.Where(x => !x.IsDedicated).Count();
        }
    }
}
