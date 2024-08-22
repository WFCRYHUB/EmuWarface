using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;

namespace EmuWarface.Xmpp.Query
{
    public static class CreateAuthorizationToken
    {
        [Query(IqType.Get, "create_authorization_token")]
        public static void Serializer(Client client, Iq iq)
        {

        }
    }
}

