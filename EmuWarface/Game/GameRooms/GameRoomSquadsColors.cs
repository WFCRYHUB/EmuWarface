using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
	public class GameRoomSquadsColors : GameRoomExtension
	{
		private GameRoomCore	_rCore;

		private Dictionary<int, string> _squadColors = new Dictionary<int, string>();

		public GameRoomSquadsColors(GameRoom room)
        {
			_rCore = room.GetExtension<GameRoomCore>();
		}

		public void UpdateSquad(string group_id)
        {
			if (string.IsNullOrEmpty(group_id))
				return;

			if (_rCore.Players.Count(x => x.Profile.RoomPlayer.GroupId == group_id) > 1)
			{
				if (_squadColors.ContainsValue(group_id))
					return;

				for(int i = 1; i < 9; i++)
                {
					if (_squadColors.ContainsKey(i))
						continue;

					_squadColors.Add(i, group_id);
					break;
				}
			}
            else
            {
				_squadColors.Remove(_squadColors.FirstOrDefault(x => x.Value == group_id).Key);
            }

			Update();
        }

		public override XmlElement Serialize()
		{
			XmlElement squads_colors = Xml.Element("squads_colors")
				.Attr("revision", Revision);

			foreach(var squad_color in _squadColors)
            {
				squads_colors.Child(Xml.Element("squad_color").Attr("id", squad_color.Value).Attr("color", squad_color.Key));
			}
			/*squads_colors.Child(Xml.Element("squad_color").Attr("id", "d41e8497-53c4-43e5-a4b4-3c68d759445d").Attr("color", "1"));
			squads_colors.Child(Xml.Element("squad_color").Attr("id", "4").Attr("color", "4294907157"));
			squads_colors.Child(Xml.Element("squad_color").Attr("id", "2").Attr("color", "4294907157"));*/

			return squads_colors;
		}
	}
}
