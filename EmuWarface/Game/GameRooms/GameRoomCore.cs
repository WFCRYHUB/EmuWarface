using EmuWarface.Core;
using EmuWarface.Game.Enums;

using EmuWarface.Xmpp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
    public class GameRoomCore : GameRoomExtension
	{
		private string _name;
		private string _private			= "0";
		private string _teamsSwitched	= "0";
		private string _teamBalanced	= "0";
		private string _canPause		= "0";
		private string _minReadyPlayers = "1";

		public List<Client> Players			{ get; set; }	= new List<Client>();
		public List<Client> PlayersReserved { get; set; }	= new List<Client>();

		/*private List<Client>	_players = new List<Client>();
		public List<Client>	Players
		{
			get
			{
				_players.RemoveAll(x => x.Presence == PlayerStatus.Logout);
				return _players;
			}
			set
			{
				_players = value;
			}
		}
		private List<Client> _playersReserved = new List<Client>();
		public List<Client> PlayersReserved
		{
			get
			{
				_playersReserved.RemoveAll(x => x.Presence == PlayerStatus.Logout);
				return _playersReserved;
			}
			set
			{
				_playersReserved = value;
			}
		}*/
		public List<ulong>	InvitedPlayers	{ get; set; } = new List<ulong>();

		public Dictionary<ulong, RoomPlayerRemoveReason> LeftPlayers = new Dictionary<ulong, RoomPlayerRemoveReason>();

		public int PlayersWarfaceCount		=> Players.Count(x => x.Profile.RoomPlayer.TeamId == Team.Warface);
		public int PlayersBlackwoodCount	=> Players.Count(x => x.Profile.RoomPlayer.TeamId == Team.Blackwood);

		public GameRoomCore(string name, int minReadyPlayers)
        {
			Name = name;
			MinReadyPlayers = minReadyPlayers;
		}

		public string Name
		{
			get { return _name; }
			set 
			{
				//TODO проверка на символы
				if (!string.IsNullOrEmpty(value) && (value.Length > 0 || value.Length < 32))
					_name = value;
			}
		}
		public bool TeamsSwitched
		{
			get { return _teamsSwitched == "1" ? true : false; }
			set { _teamsSwitched = Convert.ToInt32(value).ToString(); }
		}
		public bool Private
		{
			get { return _private == "1" ? true : false; }
			set { _private = Convert.ToInt32(value).ToString(); }
		}
		public bool TeamBalanced
		{
			get { return _teamBalanced == "1" ? true : false; }
			set { _teamBalanced = Convert.ToInt32(value).ToString(); }
		}
		public bool CanPause
		{
			get { return _canPause == "1" ? true : false; }
			set { _canPause = Convert.ToInt32(value).ToString(); }
		}
		public int MinReadyPlayers
		{
			get { return Convert.ToInt32(_minReadyPlayers); }
			set { _minReadyPlayers = value.ToString(); }
		}
		public bool CanStart => Players.Count(x => x.Profile?.RoomPlayer?.Status == RoomPlayerStatus.Ready) >= MinReadyPlayers;

		public override XmlElement Serialize()
		{
			//<core teams_switched='0' room_name='Комната игрока пуэрман' private='0' players='15' can_start='0' team_balanced='1' min_ready_players='6' can_pause='0' revision='1356'>
			XmlElement core = Xml.Element("core")
				.Attr("teams_switched",		_teamsSwitched)
				.Attr("room_name",			_name)
				.Attr("private",			_private)
				.Attr("players",			Players.Count)
				.Attr("can_start",			Convert.ToInt32(CanStart))
				.Attr("team_balanced",		_teamBalanced)
				.Attr("min_ready_players",	_minReadyPlayers)
				.Attr("can_pause",			_canPause)
				.Attr("revision",			Revision);

			XmlElement players			= Xml.Element("players");
			XmlElement playersReserved	= Xml.Element("playersReserved");

            lock (Players)
            {
				foreach (var player in Players)
				{
					players.Child(player.RoomSerialize());
				}
			}

			lock (PlayersReserved)
			{
				foreach (var playerReserved in PlayersReserved)
				{
					playersReserved.Child(playerReserved.RoomSerialize());
				}
			}

			XmlElement team_colors = Xml.Element("team_colors");
			team_colors.Child(Xml.Element("team_color").Attr("id", "1").Attr("color", "4294907157"));
			team_colors.Child(Xml.Element("team_color").Attr("id", "2").Attr("color", "4279655162"));

			var room_left_players = Xml.Element("room_left_players");

            lock (LeftPlayers)
            {
				foreach (var left_player in LeftPlayers)
				{
					var player = Xml.Element("player")
						.Attr("profile_id", left_player.Key)
						.Attr("left_reason", (int)left_player.Value);

					room_left_players.Child(player);
				}
			}

			core.Child(players);
			core.Child(playersReserved);
			core.Child(team_colors);
			core.Child(room_left_players);

			return core;
		}
	}
}
