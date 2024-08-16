using EmuWarface.Core;
using EmuWarface.Game.Enums;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EmuWarface.Game
{
    public static class Chat
    {
        public static string GetChannelId(Client client, LobbyChatChannel channel)
        {
            switch (channel)
            {
                case LobbyChatChannel.Global:
                    return string.Format("global.{0}", client.Channel.Resource);
                case LobbyChatChannel.Room:
                    return string.Format("room.{0}", client.Profile.RoomPlayer?.Room?.Id);
                case LobbyChatChannel.Team:
                    return string.Format("room.{0}.{1}", client.Profile.RoomPlayer?.Room?.Id, client.Profile.RoomPlayer?.TeamId);
                case LobbyChatChannel.Clan:
                    //return string.Format("clan.{0}", client.ClanId);
                    //TODO
                    return string.Format("clan.{0}", client.Profile.ClanId);
                case LobbyChatChannel.Observer:
                    return string.Format("room.observer.{0}", client.Profile.RoomPlayer?.Room?.Id);
                default:
                    return "default";
            }
        }
    }
}
