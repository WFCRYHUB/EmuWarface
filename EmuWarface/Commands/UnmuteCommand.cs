using EmuWarface.Core;
using EmuWarface.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuWarface.Commands
{
    public class UnmuteCommand : ICmd
    {
        public Permission MinPermission => Permission.Moderator;
        public string Usage => "unmute";
        public string Example => "unmute <nickname>";
        public string[] Names => new[] { "unmute", "um" };

        public string OnCommand(Permission permission, string[] args)
        {
            if (args.Length == 0)
                return $"Invalid arguments.\nExample:\n{Example}";

            string nickname = args[0];

            Profile profile = Profile.GetProfileForNickname(nickname);
            if (profile == null)
            {
                return $"Player with nickname '{nickname}' not found.";
            }

            ulong user_id = Profile.GetUserId(profile.Id);

            SQL.QueryRead($"DELETE FROM emu_mutes WHERE user_id={user_id}");
            return $"Player with nickname '{nickname}' has unmuted.";
        }
    }
}
