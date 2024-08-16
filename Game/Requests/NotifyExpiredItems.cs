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

namespace EmuWarface.Game.Requests
{
    public static class NotifyExpiredItems
    {
        [Query(IqType.Get, "notify_expired_items")]
        public static void NotifyExpiredItemsSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            //TODO
        }
    }
}
