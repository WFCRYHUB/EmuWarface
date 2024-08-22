using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class PlayerLeaveBan
    {
        /*
<player_leave_ban>
<profile profile_id="19" game_room_type="pvp_public" game_mode="tdm" />
</player_leave_ban>
         */

        [Query(IqType.Get, "player_leave_ban")]
        public static void PlayerLeaveBanSerializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            var q = iq.Query;
            //TODO
        }
    }
}

