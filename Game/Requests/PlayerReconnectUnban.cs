using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Requests
{
    public static class PlayerReconnectUnban
    {
        [Query(IqType.Get, "player_reconnect_unban")]
        public static void PlayerReconnectUnbanSerializer(Client client, Iq iq)
        {
            //TODO

        }
    }
}
