using EmuWarface;
using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace EmuWarface
{
    public static class Server
    {
        private static Socket _socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        public static X509Certificate2 Certificate = new X509Certificate2("Config/cert.pfx");

        public static List<Invitation>  Invitations { get; set; } = new List<Invitation>();
        public static List<Client>      Clients     { get; set; } = new List<Client>();

        public static List<MasterServer> Channels = new List<MasterServer>();
        public static List<Client> Dedicateds
        {
            get
            {
                lock (Clients)
                {
                    return Clients.Where(x => x.IsDedicated).ToList();
                }
            }
        }
        public static Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        public static uint DedicatedSeed;

        public static void Init()
        {
            InitChannels();

            _socket.Bind(new IPEndPoint(IPAddress.Any, Config.Settings.Port));
            _socket.Listen(10);

            Log.Info($"Server started on {Config.Settings.Port} port");
            _socket.BeginAccept(OnAccept, null);
        }

        public static void InitChannels()
        {
            foreach (var mscfg in Config.MasterServers)
            {
                Channels.Add(new MasterServer(mscfg.ServerId, mscfg.Resource, mscfg.Channel, mscfg.RankGroup, mscfg.MinRank, mscfg.MaxRank, mscfg.Bootstrap));
            }
        }

        private static void OnAccept(IAsyncResult Result)
        {
            _socket.BeginAccept(OnAccept, null);
            try
            {
                Client client = new Client(_socket.EndAccept(Result));
                Log.Info(client.IPAddress + " connected");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                //API.SendAdmins(e.ToString());
            }

            GC.Collect(0);
        }
    }
}
