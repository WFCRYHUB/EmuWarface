using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Timers;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
	public class GameRoomAutoStart : GameRoomExtension
	{
		private GameRoom _room;
		private GameRoomCore _rCore;
		private GameRoomSession _rSession;

#if DEBUGLOCAL
		private int _endSessionTimeout = 30;
		private int _timeout = 15;
#else
		private int _endSessionTimeout = 35;
		private int _timeout = 60;
#endif

		public bool AutoStartTimeout			{ get; set; }
		public int AutoStartTimeoutLeft			{ get; set; }
		//TODO ?
		public bool CanManualStart				{ get; set; }
		public int JoinedIntermissionTimeout	{ get; set; }

		public override XmlElement Serialize()
		{
			return Xml.Element("auto_start")
				.Attr("auto_start_timeout",				AutoStartTimeout ? 1 : 0)
				.Attr("auto_start_timeout_left",		AutoStartTimeoutLeft)
				.Attr("can_manual_start",				CanManualStart ? 1 : 0)
				.Attr("joined_intermission_timeout",	JoinedIntermissionTimeout)
				.Attr("revision", Revision);
		}

		public GameRoomAutoStart(GameRoom room)
		{
			_room		= room;
			_rCore		= _room.GetExtension<GameRoomCore>(false);
			_rSession	= _room.GetExtension<GameRoomSession>(false);

			Start();
		}

		private void UpdateAutoStart()
		{
			if (!AutoStartTimeout)
            {
				if (_rSession.Status == SessionStatus.None || _rCore.MinReadyPlayers > _rCore.Players.Count)
				{
					Stop();
				}
                else
                {
					Start();
				}
			}

			if (AutoStartTimeoutLeft-- <= 0)
            {
				AutoStartTimeoutLeft = 0;

				if (_rCore.MinReadyPlayers > _rCore.Players.Count)
				{
					Stop();
					return;
				}

				_room.MissionLoad();
				return;
			}

			EmuExtensions.Delay(1).ContinueWith(task => UpdateAutoStart());
		}

		public void Stop()
		{
			AutoStartTimeout		= false;
			AutoStartTimeoutLeft	= 0;

			Update();
		}

		public void Start()
		{
			if (!AutoStartTimeout)
			{
				AutoStartTimeout = true;
				AutoStartTimeoutLeft = _timeout;

				Update();
				UpdateAutoStart();
			}
		}

		public void EndGame()
		{
			EmuExtensions.Delay(_endSessionTimeout).ContinueWith(task => Start());
		}
	}
}
