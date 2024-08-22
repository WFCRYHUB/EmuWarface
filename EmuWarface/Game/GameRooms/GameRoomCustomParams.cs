using EmuWarface.Game.Enums;

using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
	public class GameRoomCustomParams : GameRoomExtension
	{
		private GameRoom _room;

		private byte _classRestriction = byte.MaxValue;
		private string _mode => _type.Contains("PvE") ? _room.GetExtension<GameRoomMission>(false).Mission.MissionType : _room.GetExtension<GameRoomMission>(false).Mode;
		private string _type => _room.Type.ToString();

		private Dictionary<string, string> _restrictions = new Dictionary<string, string>();

		/*<custom_params friendly_fire='0' enemy_outlines='0' auto_team_balance='1' dead_can_chat='1' join_in_the_process='1' 
			* max_players='16' round_limit='0' preround_time='-1' class_restriction='253' inventory_slot='2113929215' 
			* locked_spectator_camera='0' high_latency_autokick='1' overtime_mode='0' revision='502'/>*/

		//<gameroom_open>
		//     <class_rifleman enabled="1" class_id="0" />
		//     <class_heavy enabled="1" class_id="1" />
		//     <class_engineer enabled="1" class_id="4" />
		//     <class_medic enabled="1" class_id="3" />
		//     <class_sniper enabled="1" class_id="2" />
		//</gameroom_open>

		public Class ClassRestriction { get; private set; }

		public GameRoomCustomParams(GameRoom room)
		{
			_room = room;

			//SetDefaultRestrictions();
			//SetRestrictions(GameRestrictionSystem.GetDefaultRestrictions(channel, mode, type), channel, mode, type);
		}

		public string GetCurrentRestriction(string kind) 
		{
            try
            {
				return _restrictions.FirstOrDefault(x => x.Key == kind).Value;
			}
            catch
            {
				return null;
			}
		}

		public void SetDefaultRestrictions()
		{
			ClassRestriction = Class.None;
			_classRestriction = byte.MaxValue;
			SetRestrictions(GameRestrictionSystem.GetDefaultRestrictions(_mode, _type));
		}

		public void SetRestrictions(XmlElement q)
		{
			foreach (XmlAttribute restriction in q.Attributes)
			{
				SetRestriction(restriction.Name, restriction.Value);
			}

			_classRestriction = byte.MaxValue;
			ClassRestriction = Class.None;

			foreach (XmlElement restriction in q.ChildNodes)
			{
				//TODO test
				/*switch (restriction.Name)
				{
					case "class_rifleman":
						_classRestriction -= restriction.GetAttribute("enabled") == "0" ? (byte)Class.Rifleman : (byte)0;
						break;
					case "class_heavy":
						_classRestriction -= restriction.GetAttribute("enabled") == "0" ? (byte)Class.Heavy : (byte)0;
						break;
					case "class_engineer":
						_classRestriction -= restriction.GetAttribute("enabled") == "0" ? (byte)Class.Engineer : (byte)0;
						break;
					case "class_medic":
						_classRestriction -= restriction.GetAttribute("enabled") == "0" ? (byte)Class.Medic : (byte)0;
						break;
					case "class_sniper":
						_classRestriction -= restriction.GetAttribute("enabled") == "0" ? (byte)Class.Recon : (byte)0;
						break;
				}*/
				SetRestriction(restriction.Name, restriction.GetAttribute("enabled"));

				//_classRestriction -= restriction.GetAttribute("enabled") == "0" ? (byte)(1 << int.Parse(restriction.GetAttribute("class_id"))) : (byte)0;
			}
		}

		public void SetRestrictions(Dictionary<string, string> restrictions)
		{
			foreach (var restriction in restrictions)
			{
				SetRestriction(restriction.Key, restriction.Value);
			}
		}

		public bool SetRestriction(string kind, string value)
		{
			//TODO temp
			if (kind == "inventory_slot")
			{
				if (_type.Contains("PvE"))
				{
					_restrictions[kind] = "34326183935";
				}
				else
				{
					_restrictions[kind] = value;
				}
				//if (!_type.Contains("PvE"))
				//	_restrictions[kind] = value;

				return true;
			}

			if (GameRestrictionSystem.ValidateRestriction(_mode, _type, kind, value))
            {
				switch (kind)
                {
					case "class_rifleman":
						if(value == "0")
                        {
							ClassRestriction = ClassRestriction | Class.Rifleman;
							_classRestriction -= (byte)Class.Rifleman;
						}
						break;
					case "class_heavy":
						if (value == "0")
						{
							ClassRestriction = ClassRestriction | Class.Heavy;
							_classRestriction -= (byte)Class.Heavy;
						}
						break;
					case "class_engineer":
						if (value == "0")
						{
							ClassRestriction = ClassRestriction | Class.Engineer;
							_classRestriction -= (byte)Class.Engineer;
						}
						break;
					case "class_medic":
						if (value == "0")
						{
							ClassRestriction = ClassRestriction | Class.Medic;
							_classRestriction -= (byte)Class.Medic;
						}
						break;
					case "class_sniper":
						if (value == "0")
						{
							ClassRestriction = ClassRestriction | Class.Recon;
							_classRestriction -= (byte)Class.Recon;
						}
						break;
					default:
						_restrictions[kind] = value;
						break;
				}

				//_restrictions[kind] = value;
				return true;
			}

			return false;
		}

		public override XmlElement Serialize()
		{
			var custom_params = Xml.Element("custom_params");

			foreach (var restriction in _restrictions)
			{
				custom_params.Attr(restriction.Key, restriction.Value);
			}

			custom_params.Attr("class_restriction", _classRestriction);

			return custom_params.Attr("revision", Revision);
			/*byte classRestriction = byte.MaxValue;
			//int inventory_slot = long.MaxValue;

			foreach (var restriction in Restrictions)
			{
				switch (restriction.Key)
				{
					case "class_rifleman":
						classRestriction -= restriction.Value == "0" ? (byte)Class.Rifleman : (byte)0;
						break;
					case "class_heavy":
						classRestriction -= restriction.Value == "0" ? (byte)Class.Heavy : (byte)0;
						break;
					case "class_engineer":
						classRestriction -= restriction.Value == "0" ? (byte)Class.Engineer : (byte)0;
						break;
					case "class_medic":
						classRestriction -= restriction.Value == "0" ? (byte)Class.Medic : (byte)0;
						break;
					case "class_sniper":
						classRestriction -= restriction.Value == "0" ? (byte)Class.Recon : (byte)0;
						break;
					default:
						custom_params.Attr(restriction.Key, restriction.Value);
						break;
				}
			}

			custom_params.Attr("class_restriction", classRestriction);*/

			//return custom_params.Attr("revision", Revision);
		}
	}
}
