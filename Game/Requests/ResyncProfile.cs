using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class ResyncProfile
    {
        [Query(IqType.Get, "resync_profile")]
        public static void Serializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            iq.SetQuery(client.Profile.ResyncProfie());
            client.QueryResult(iq);
        }
    }
}