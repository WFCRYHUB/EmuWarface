using EmuWarface.Core;
using EmuWarface.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuWarface.Commands
{
    public class SetExpCommand : ICmd
    {
        public Permission MinPermission => Permission.Moderator;
        public string Usage => "setexp <nickname> <exp>";
        public string Example => "setexp user1 23046000";
        public string[] Names => new[] { "setexp", "exp" };

        public string OnCommand(Permission permission, string[] args)
        {
            if (args.Length != 2)
                return $"Invalid arguments.\nExample:\n{Example}";

            string nickname = args[0];
            int exp;
            if(!int.TryParse(args[1], out exp))
            {
                return $"Invalid experience ('{args[1]}' not a number).";
            }

            Profile profile = Profile.GetProfileForNickname(nickname);
            if (profile == null)
            {
                return $"Player with nickname '{nickname}' not found.";
            }

            profile.Experience = exp;
            profile.CheckRankUpdated();
            profile.Update();

            lock (Server.Clients)
            {
                Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nickname)?.ResyncProfie();
            }

            return $"Player with nickname '{nickname}' has been successfully given experience.";
        }
    }
}
