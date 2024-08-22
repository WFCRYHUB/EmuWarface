using EmuWarface.Core;
using EmuWarface.Xmpp;

namespace EmuWarface.Xmpp.Query
{
    public static class GetMasterServer
    {
        [Query(IqType.Get, "get_master_server")]
        public static void GetMasterServerSerializer(Client client, Iq iq)
        {
            foreach (MasterServer channel in Server.Channels)
            {
                if (channel.ChannelType == iq.Query.GetAttribute("channel")
                    //костыль TODO
                    || iq.Query.GetAttribute("channel") == "pvp_newbie" && channel.ChannelType == "pvp_skilled")
                {
                    iq.SetQuery(Xml.Element("get_master_server").Attr("resource", channel.Resource).Attr("load_index", "255"));
                    client.QueryResult(iq);
                    return;
                }
            }

            throw new QueryException(1);
        }
    }
}

