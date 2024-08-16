using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace EmuWarface.Core
{
	public class MasterServer
	{
		public int ServerId;
		public int MinRank;
		public int MaxRank;
		public string Resource;
		public string ChannelType;
		public string RankGroup;
		public string Bootstrap;
		public string Jid => "masterserver@warface/" + Resource;
		public int Online => Server.Clients.Count(x => x.Presence != PlayerStatus.Logout && x.Presence != PlayerStatus.Offline && x.Channel == this);
		public double Load => (double)Online / 100;

		public List<GameRoom>	Rooms	{ get; set; } = new List<GameRoom>();

		//private static double _totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
		//private static Process _p = Process.GetCurrentProcess();
		//public static double Load => ((double)_p.WorkingSet64 * 100 / _totalMemory) * 0.01;

		public MasterServer(int ServerId, string Resource, string Channel, string RankGroup, int MinRank, int MaxRank, string Bootstrap)
		{
			this.ServerId = ServerId;
			this.Resource = Resource;
			this.ChannelType = Channel;
			this.RankGroup = RankGroup;
			this.MinRank = MinRank;
			this.MaxRank = MaxRank;
			this.Bootstrap = Bootstrap;

			Log.Info("[MasterServer] Channel '{0}' started", Resource);
		}

		public static ulong GetUserId(Jid online_id)
		{
			if (online_id.Resource == "GameClient")
				return ulong.Parse(online_id.Node);

			return 0;
		}

		public XmlElement Serialize()
		{
			XmlElement server = Xml.Element("server")
				.Attr("resource", Resource)
				.Attr("server_id", ServerId)
				.Attr("channel", ChannelType)
				.Attr("rank_group", RankGroup)
				.Attr("load", Load.ToString("F3").Replace(',', '.'))
				.Attr("online", Online)
				.Attr("min_rank", MinRank)
				.Attr("max_rank", MaxRank)
				.Attr("bootstrap", Bootstrap);

			var stats = Xml.Element("load_stats");
			stats.Child(Xml.Element("load_stat").Attr("type", "quick_play")
				.Attr("value", "255"));
			stats.Child(Xml.Element("load_stat").Attr("type", "survival")
				.Attr("value", "255"));
			stats.Child(Xml.Element("load_stat").Attr("type", "pve")
				.Attr("value", "255"));

			server.Child(stats);
			return server;
		}
	}
}