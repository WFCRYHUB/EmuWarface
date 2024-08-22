using EmuWarface.Core;
using EmuWarface.Game.Enums;

using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
    public class GameRoomSession : GameRoomExtension
	{
		private static long _seedId = 0;

		public long Id					{ get; set; }
		public long StartTime			{ get; set; }
		public float GameProgress		{ get; set; }
		public int Team1StartScore		{ get; set; }
		public int Team2StartScore		{ get; set; }
		public SessionStatus Status		{ get; set; }

		public DedicatedServer Dedicated	{ get; set; }
		public XmlElement Telemetry			{ get; set; }
		//<session id='3461684861154490475' status='2' game_progress='0' start_time='1500903531' team1_start_score='0' 
		//team2_start_score ='0' revision='914'/>

		public void StartSession()
        {
			//TODO test вроде как PreGame
			Id			= ++_seedId;
			StartTime	= DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			Telemetry = Xml.Element("telemetry_stream")
				.Child(Xml.Element("players"))
				.Child(Xml.Element("teams"))
				.Child(Xml.Element("rounds"));
		}

		public void EndSession()
		{
			Id				= 0;
			StartTime		= 0;
			GameProgress	= 0;
			Team1StartScore = 0;
			Team2StartScore = 0;
			Dedicated		= null;
			Status			= SessionStatus.None;
		}

		public override XmlElement Serialize()
		{
			return Xml.Element("session")
				.Attr("id",					Id)
				.Attr("status",				(int)Status)
				.Attr("game_progress",		GameProgress)
				.Attr("start_time",			StartTime)
				.Attr("team1_start_score",	Team1StartScore)
				.Attr("team2_start_score",	Team2StartScore)
				.Attr("revision",			Revision);
		}
	}
}
