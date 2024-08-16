using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
	public class GameRoomClanWar : GameRoomExtension
	{
		public string ClanFirst		{ get; set; }
		public string ClanSecond	{ get; set; }

		public GameRoomClanWar(string clan)
        {
			ClanFirst = clan;
		}

		public override XmlElement Serialize()
		{
			return Xml.Element("clan_war")
				.Attr("clan_1", ClanFirst)
				.Attr("clan_2", ClanSecond)
				.Attr("revision", Revision);
		}
	}
}
