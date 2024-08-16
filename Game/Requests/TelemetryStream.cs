using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.IO;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class TelemetryStream
    {
        [Query(IqType.Get, "telemetry_stream")]
        public static void TelemetryStreamSerializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            var q = iq.Query;

            if(client.Dedicated.Room == null)
            {
                client.Dedicated.MissionUnload();
                //throw new ServerException("Room is null, reconnect dedicated");
                return;
            }

            var rSession    = client.Dedicated.Room.GetExtension<GameRoomSession>();
            var rCore       = client.Dedicated.Room.GetExtension<GameRoomCore>();

            //Замена знаков
            var telemetryData = q.InnerText.Replace("&lt;", "<").Replace("&gt;", ">").Split("\n");

            for (int i = 0; i < telemetryData.Length; i++)
            {
                XmlElement currentData = null;
                try
                {
                    currentData = Xml.Parse(telemetryData[i]);
                }
                catch
                {
                    continue;
                }

                var key = telemetryData[i + 1].Split("{1}[")[1].Split("]")[0].Split(",");

                var keyType = int.Parse(key[0]) + int.Parse(key[1]) + int.Parse(key[2]);
                var keyId = int.Parse(key[4]);

                XmlElement selectedData = null;

                switch (keyType)
                {
                    case -1:
                        selectedData = rSession.Telemetry;
                        break;
                    case 0:
                        selectedData = rSession.Telemetry["rounds"];
                        break;
                    case 5:
                        selectedData = rSession.Telemetry["players"];
                        break;
                    case 2:
                        selectedData = rSession.Telemetry["teams"];
                        break;
                }

                if (selectedData == null)
                    continue;

                XmlElement sessionData = null;

                foreach (XmlElement elem in selectedData.ChildNodes)
                {
                    if (elem.GetAttribute("_id") == keyId.ToString())
                        sessionData = elem;
                }

                if (sessionData == null)
                {
                    currentData.Attr("_id", keyId);
                    selectedData.Child(currentData);
                }
                else
                {
                    //Складывание атрибутов
                    foreach (XmlAttribute attr in currentData.Attributes)
                    {
                        sessionData.Attr(attr.Name, currentData.GetAttribute(attr.Name));
                    }

                    //Сложение тамлайнов
                    var timelines   = currentData["timelines"];
                    var sTimelines  = sessionData["timelines"];

                    foreach (XmlElement timeline in timelines.GetElementsByTagName("timeline"))
                    {
                        ////console.log(tf_timeline.attrs.name);
                        XmlElement selectedTimeline = null;
                        foreach (XmlElement sTimeline in sTimelines.ChildNodes)
                        {
                            if (sTimeline.GetAttribute("name") == timeline.GetAttribute("name"))
                            {
                                selectedTimeline = sTimeline;
                            }
                        }
                        if (selectedTimeline == null)
                        {
                            ////console.log("TMLN NF");
                            sTimelines.Child(timeline);
                        }
                        else
                        {
                            ////console.log("TMLN FF");
                            foreach (XmlElement value in timeline.GetElementsByTagName("val"))
                            {
                                selectedTimeline.Child(value);
                            }
                        }


                    }
                }
            }

            if (q.GetAttribute("finalize") == "1")
            {
                //File.WriteAllText(string.Format("Logs/telemetry_{0}.xml", rSession.StartTime), rSession.Telemetry.OuterXml);

                //Log.Info(rSession.Telemetry.OuterXml);
                Log.Info("[GameRoom] Telemetry Finalized (session_id: {0}, dedicated: {1})", rSession.Id, client.Jid.Resource);

                StatsManager.CalculateSessionStats(rSession.Telemetry);

                //TODO temp
                //foreach(var target in rCore.Players)
                //{
                    //XmlElement get_player_stats = Xml.Element(iq.Query.LocalName);
                    //target.Profile.Stats.ForEach(stat => get_player_stats.Child(stat.Serialize()));

                //    target.QueryGet(PlayerStat.GetPlayerStats(target.Profile.Stats));
                //}
            }

            iq.SetQuery(Xml.Element("telemetry_stream"));
            client.QueryResult(iq);
        }
    }
}
