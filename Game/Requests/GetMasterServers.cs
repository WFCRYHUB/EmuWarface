using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class GetMasterServers
    {
        [Query(IqType.Get, "get_master_servers")]
        public static void GetMasterServersSerializer(Client client, Iq iq)
        {
            XmlElement get_master_servers = Xml.Element("get_master_servers");

            XmlElement masterservers = Xml.Element("masterservers");
            foreach (MasterServer channel in Server.Channels)
            {
                masterservers.Child(channel.Serialize());
            }

            get_master_servers.Child(masterservers);

            iq.SetQuery(get_master_servers);
            client.QueryResult(iq);
        }
    }
}
