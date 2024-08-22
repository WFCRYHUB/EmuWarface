using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using System;
using System.Linq;

namespace EmuWarface.Xmpp.Query
{
    public static class InvitationAccept
    {
        [Query(IqType.Get, "invitation_accept")]
        public static void InvitationAcceptSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            var invitation = Server.Invitations.FirstOrDefault(x => x.Id.ToString() == q.GetAttribute("ticket"));
            if (invitation != null)
            {
                invitation.Status = Utils.ParseEnum<UserInvitationStatus>(q.GetAttribute("result"));

                if (invitation.Status == UserInvitationStatus.Accepted)
                    invitation.Sender?.Profile?.Room?.GetExtension<GameRoomCore>()?.InvitedPlayers?.Add(client.ProfileId);

                invitation.Result();
                lock (Server.Invitations)
                {
                    Server.Invitations.Remove(invitation);
                }
            }

            iq.SetQuery(Xml.Element("invitation_accept"));
            client.QueryResult(iq);
        }
    }
}