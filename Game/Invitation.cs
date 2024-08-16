using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public class Invitation
    {
		private static ulong _seedId;

		public ulong	Id			{ get; set; }
		public string	GroupId		{ get; set; }
		public Client	Sender		{ get; set; }
		public Client	Target		{ get; set; }
		public bool IsFollow		{ get; set; }

		public UserInvitationStatus Status = UserInvitationStatus.InvalidState;

		public Invitation(Client sender, Client target, bool isFollow, string groupId)
		{
			Id = _seedId++;

			GroupId = groupId;
			Target = target;
			Sender = sender;
			IsFollow = isFollow;
		}

		public void Request()
        {
			var room = Sender.Profile.Room;

			XmlElement invitation_request = Xml.Element("invitation_request")
				.Attr("from",			Sender.Profile.Nickname)
				.Attr("ms_resource",	Sender.Channel.Resource)
				.Attr("room_id",		room.Id)
				.Attr("ticket",			Id)
				.Attr("is_follow",		IsFollow ? 1 : 0);

			invitation_request.Child(Profile.GetInitiatorInfo(Sender.ProfileId));
			invitation_request.Child(room.Serialize(true));

			//Iq iq = new Iq(IqType.Get, Target.Jid, Sender.Channel.Jid);
			//Target.QueryGet(iq.SetQuery(invitation_request));
			Target.QueryGet(invitation_request);
		}

		public void Result()
		{
			var room = Sender.Profile.Room;

			//TODO user_id это profile_id или нет?

			XmlElement invitation_result = Xml.Element("invitation_result")
				.Attr("result",		(int)Status)
				.Attr("user",		Target.Profile.Nickname)
				.Attr("user_id",	Target.UserId)
				.Attr("is_follow",	IsFollow ? 1 : 0);

			//Iq iq = new Iq(IqType.Get, Sender.Jid, Target.Channel.Jid);
			//Sender.QueryGet(iq.SetQuery(invitation_result));
			Sender.QueryGet(invitation_result);
		}
	}
}
