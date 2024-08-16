using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class InvitationSend
    {
        /*
         <iq to='masterserver@warface/pvp_skilled_001' id='uid00000099' type='get' from='3@warface/GameClient' xmlns='jabber:client'>
<query xmlns='urn:cryonline:k01'>
<invitation_send nickname='PekarMeow' is_follow='1' group_id='65d19f79-4440-4d7d-bd3e-d25a6e1b8d7f'/>
</query>
</iq>
         */

        [Query(IqType.Get, "invitation_send")]
        public static void InvitationSendSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            if (client.Profile.Room == null)
                throw new QueryException(UserInvitationStatus.ServiceError);

            var q = iq.Query;

            var group_id = q.GetAttribute("group_id");
            var nickname = q.GetAttribute("nickname");
            var is_follow = q.GetAttribute("is_follow");

            var room = client.Profile.Room;
            if (room == null)
                throw new QueryException(UserInvitationStatus.ServiceError);

            //if (room.IsPrivate)
            //    throw new QueryException(UserInvitationStatus.PrivateRoom);
            Client target = null;

            lock (Server.Clients)
            {
                target = Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nickname);
            }
            if (target == null)
                //throw new QueryException(UserInvitationStatus.TargetInvalid);
                throw new QueryException(UserInvitationStatus.UserOffline);

            if (client.Channel.MinRank > target.Profile.GetRank() || client.Channel.MaxRank < target.Profile.GetRank())
                throw new QueryException(UserInvitationStatus.RankRestriction);

            var invitation = new Invitation(client, target, is_follow == "1" ? true : false, group_id);
            lock (Server.Invitations)
            {
                Server.Invitations.Add(invitation);
            }

            invitation.Request();

            //UserInvitationStatus.FullRoom

            iq.SetQuery(Xml.Element("invitation_send"));
            client.QueryResult(iq);
        }
    }
}
