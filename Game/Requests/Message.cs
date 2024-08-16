using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class Message
    {
        [Query(IqType.Get, "message")]
        public static void MessageSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new QueryException(1);

            var q = iq.Query;

            var nick = q.GetAttribute("nick");
            var message = q.GetAttribute("message");

            var unmute_time = Profile.GetMuteTime(client.UserId);
            if (unmute_time != -1)
                return;

            if (!GameData.ValidateInputString("ChatText", message))
            {
                //API.Mute(client.Profile.Nickname, "3.1", "1h");
                return;
            }
            Client target = null;

            lock (Server.Clients)
            {
                target = Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nick);
            }
            if(target != null)
            {
                q.Attr("from", client.Profile.Nickname);
                target.Send(iq);

                Log.Chat("{0}({1}) => {2}({3}): {4}", client.Profile.Nickname, client.UserId, nick, target.UserId, message);
            }

            client.QueryResult(iq);
        }
    }
}

