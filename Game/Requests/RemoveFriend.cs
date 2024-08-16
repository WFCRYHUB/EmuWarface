using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class RemoveFriend
    {
        [Query(IqType.Get, "remove_friend")]
        public static void RemoveFriendSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            ulong target_id = Profile.GetProfileIdForNickname(iq.Query.GetAttribute("target"));
            if (target_id == 0)
                throw new QueryException(1);

            Friend.RemoveFriend(client.ProfileId, target_id);

            Client target = null;

            lock (Server.Clients)
            {
                target = Server.Clients.FirstOrDefault(x => x.ProfileId == target_id);
            }

            if (target != null)
            {
                XmlElement response = Xml.Element("remove_friend")
                    .Attr("target", client.Profile.Nickname);

                //Iq iq_rem = new Iq(IqType.Get, target.Jid, client.Channel.Jid);
                //target.QueryGet(iq_rem.SetQuery(response));
                target.QueryGet(response);
            }

            //GetFriends(Profile.GetOnlineId(target_id));
            //GetFriends(iq.From);

            client.QueryResult(iq);
        }
    }
}
