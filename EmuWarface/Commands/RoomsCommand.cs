using EmuWarface.Core;
using EmuWarface.Game.GameRooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuWarface.Commands
{
    public class RoomsCommand : ICmd
    {
        public Permission MinPermission => Permission.Moderator;
        public string Usage => "rooms";
        public string Example => "rooms";
        public string[] Names => new[] { "rooms", "r" };

        public string OnCommand(Permission permission, string[] args)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var channel in Server.Channels)
            {
                sb.AppendLine(channel.Resource);

                lock (channel.Rooms)
                {
                    foreach (var room in channel.Rooms)
                    {
                        var rCore = room.GetExtension<GameRoomCore>();
                        var rCustomParams = room.GetExtension<GameRoomCustomParams>();

                        sb.AppendLine(string.Format(@"  ""{0}"" {1}/{2}", rCore.Name, rCore.Players.Count, rCustomParams.GetCurrentRestriction("max_players")));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
