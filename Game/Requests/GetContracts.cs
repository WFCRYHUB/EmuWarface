using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;

namespace EmuWarface.Game.Requests
{
    public static class GetContracts
    {
        [Query(IqType.Get, "get_contracts")]
        public static void GetContractsSerializer(Client client, Iq iq)
        {
            client.QueryResult(iq);
        }
    }
}
