using EmuWarface.Core;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
	public static class VoipServerStatusQuery
	{
		[Query(IqType.Get, "voip_server_status_query")]
		public static void VoipServerStatusQuerySerializer(Client client, Iq iq)
		{
            //<voip_server_status_query
            //voipEnabled='1'
            //profileId='17352061'
            //playerSip='sip:.17352061.@wfp.vivox.com'
            //muted='0'
            //correlationId=''
            //voipDomain='wfp.vivox.com'
            //voipApi='http://wfp.www.vivox.com/api2/'/>
            XmlElement voip_server_status_query = Xml.Element(iq.Query.LocalName)
			   .Attr("voipEnabled", "0")
			   .Attr("voipServerStatus", "0")
			   .Attr("profileId", client.ProfileId)
			   .Attr("playerSip", $"127.0.0.1:5222")
			   .Attr("muted", "1")
			   .Attr("correlationId", "")
			   .Attr("voipDomain", "127.0.0.1:5222")
			   .Attr("voipApi", "http://127.0.0.1:5222");

            iq.SetQuery(voip_server_status_query);
            client.QueryResult(iq);
		}
	}
}
