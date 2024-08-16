using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GameRoomOfferResponse
    {
        /*
         <gameroom_offer_response id="0" result="0" />
         */

        [Query(IqType.Get, "gameroom_offer_response")]
        public static void GameRoomOfferResponseSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            var id      = q.GetAttribute("id");
            var result  = q.GetAttribute("result");

            iq.SetQuery(Xml.Element("gameroom_offer_response"));
            client.QueryResult(iq);
        }
    }
}
