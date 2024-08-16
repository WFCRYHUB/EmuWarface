using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GetProfilePerformance
    {
		[Query(IqType.Get, "get_profile_performance")]
		public static void GameRoomOpen(Client client, Iq iq)
        {
            //if (client.Profile == null)
            //    throw new InvalidOperationException();

			XmlElement response = Xml.Element(iq.Query.LocalName);

			response.Child(Xml.Element("pvp_modes_to_complete"));
			response.Child(Xml.Element("pve_missions_performance")
				.Attr("mode", "ctf")
				.Attr("mode", "dst")
				.Attr("mode", "ptb")
				.Attr("mode", "lms")
				.Attr("mode", "ffa")
				.Attr("mode", "stm")
				.Attr("mode", "tbs")
				.Attr("mode", "dmn")
				.Attr("mode", "hnt")
				.Attr("mode", "tdm"));

			iq.SetQuery(response);
			client.QueryResult(iq);
        }
    }
}
