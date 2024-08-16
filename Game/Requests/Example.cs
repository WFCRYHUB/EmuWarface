using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;

namespace EmuWarface.Game.Requests
{
    public static class Example
    {
        [Query(IqType.Get, "example")]
        public static void ExampleSerializer(Client client, Iq iq)
        {

        }
    }
}
