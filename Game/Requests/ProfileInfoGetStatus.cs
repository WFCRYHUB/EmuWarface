using EmuWarface.Core;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class ProfileInfoGetStatus
    {
        [Query(IqType.Get, "profile_info_get_status")]
        public static void ProfileInfoGetStatusSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            var nickname = q.GetAttribute("nickname");
            Client target = null;

            lock (Server.Clients)
            {
                target = Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nickname);
            }

            if (target == null)
                return;

            q.Child(Xml.Element("profile_info")
                .Child(Xml.Element("info")
                .Attr("nickname",   nickname)
                .Attr("online_id",  target.Jid.ToString())
                .Attr("status",     (int)target.Presence)
                .Attr("rank",       target.Profile.GetRank())
                .Attr("user_id",    target.UserId)
                .Attr("profile_id", target.ProfileId)));

            client.QueryResult(iq);
        }
    }
}

