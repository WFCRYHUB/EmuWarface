using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using System;
using System.Linq;

namespace EmuWarface.Core
{
    public class DedicatedServer
    {
        public uint DedicatedId     { get; private set; }
        public SessionStatus Status { get; set; } = SessionStatus.None;

        public string   Host    { get; private set; }
        public string   Node    { get; private set; }
        public int      Port    { get; private set; }
        public Client   Client  { get; private set; }
        public GameRoom Room        { get; set; }
        public MasterServer Channel { get; set; }

        /*public long LastPingTime { get; set; }

        public bool IsReady
        {
            get
            {
                if (LastPingTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds() > 10)
                {
                    Client.Dispose();
                    return false;
                }
                return true;
            }
        }*/

        public DedicatedServer(Client client)
        {
            Client = client;
        }

        public void MissionUnload()
        {
            try
            {
                Room = null;
                Client.QueryGet(Xml.Element("mission_unload"));
            }
            catch
            {

            }
        }

        public void SetServer(SessionStatus status, string ms_resource, string host, string node, int port)
        {
            Status = status;

            Host = host;
            Node = node;
            Port = port;

            //Log.Info("[Dedicated] SetServer (status: {0}, resource: {1}, dedicated: {2})", status, ms_resource, Client.Jid.Resource);

            SetChannel(ms_resource);
        }

        private void SetChannel(string ms_resource)
        {
            if (string.IsNullOrEmpty(ms_resource))
                return;

            if (Channel != null && Channel.Resource == ms_resource)
                return;

            var channel = Server.Channels.FirstOrDefault(x => x.Resource == ms_resource);

            if (channel == null)
                throw new ServerException("Channel for dedicated not found");

            Channel = channel;
        }
    }
}
