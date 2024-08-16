using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
	public class GameRoomInGameChat : GameRoomExtension
	{
		private GameRoom _room;

		public GameRoomInGameChat(GameRoom room)
        {
			_room = room;
		}

		public override XmlElement Serialize()
		{
			XmlElement ingame_chat = Xml.Element("ingame_chat")
				.Attr("revision", Revision);

			XmlElement channel1 = Xml.Element("channel")
				.Attr("name",			"firstteam")
				.Attr("channel_id",		$"room.{_room.Id}.Warface")
				.Attr("conference_id",	"conference.warface");

			XmlElement channel2 = Xml.Element("channel")
				.Attr("name",			"secondteam")
				.Attr("channel_id",		$"room.{_room.Id}.Blackwood")
				.Attr("conference_id",	"conference.warface");

			XmlElement channel3 = Xml.Element("channel")
				.Attr("name",			"observer")
				.Attr("channel_id",		$"room.observer.{_room.Id}")
				.Attr("conference_id",	"conference.warface");

			ingame_chat.Child(channel1);
			ingame_chat.Child(channel2);
			ingame_chat.Child(channel3);

			return ingame_chat;
		}
	}
}
