using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;

namespace EmuWarface.Game.Requests
{
    public static class UpdateContracts
    {
        [Query(IqType.Get, "update_contracts")]
        public static void UpdateContractsSerializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            //TODO


        }
    }
}
