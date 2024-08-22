using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class ValidatePlayerInfo
    {
        [Query(IqType.Get, "validate_player_info")]
        public static void ValidatePlayerInfoSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            //TODO

            client.QueryResult(iq.SetQuery(Xml.Element("validate_player_info")));
        }
    }
}
