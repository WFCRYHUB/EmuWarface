using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game
{
    public static class Quickplay
    {
        public static void Started(Client client)
        {
            //<gameroom_quickplay_started uid='73a3a1ef-2965-42b1-8d86-e01a9f3c4484' time_to_maps_reset_notification='120' timestamp='1547833530' mission_hash='1829658368' content_hash='609720826'/>
            var gameroom_quickplay_started = Xml.Element("gameroom_quickplay_started")
                .Attr("uid", client.Profile.RoomPlayer.GroupId)
                .Attr("time_to_maps_reset_notification", "120")
                .Attr("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .Attr("mission_hash", "0") //TODO
                .Attr("content_hash", "0");

            client.QueryGet(gameroom_quickplay_started, "k01.warface");
        }

        public static void Succeeded(Client client)
        {
            //<gameroom_quickplay_succeeded uid='73a3a1ef-2965-42b1-8d86-e01a9f3c4484'/>
            var gameroom_quickplay_succeeded = Xml.Element("gameroom_quickplay_succeeded")
                .Attr("uid", client.Profile.RoomPlayer.GroupId);

            client.QueryGet(gameroom_quickplay_succeeded, "k01.warface");
        }

        public static void RoomOffer(GameRoom room, params Client[] clients)
        {
            if (room.Type != RoomType.PvE_Autostart && room.Type != RoomType.PvP_Autostart && room.Type != RoomType.PvP_Rating)
                throw new InvalidOperationException("room type is not quick");

            //<gameroom_offer from="МрЛоки1965Герда" room_id="5802418353523917242" token="a2f01d77-21a0-43b5-bd43-69cf906f9c0f" team_id="0" ms_resource="pvp_newbie_003_r1" id="12d07d5e-ef29-46b2-b7ce-2481b9e61bcc" silent="1">
            //TODO silent?
            var gameroom_offer = Xml.Element("gameroom_offer")
                //.Attr("from", "lol") //TODO
                .Attr("room_id", room.Id)
                .Attr("ms_resource", room.Channel.Resource)
                .Attr("id", Guid.NewGuid().ToString())
                .Attr("silent", "1");

            gameroom_offer.Child(room.Serialize(true));

            foreach (var client in clients)
            {
                gameroom_offer.Attr("from", client.Profile.Nickname); //TODO lol
                gameroom_offer.Attr("token", client.Profile.RoomPlayer.GroupId);
                gameroom_offer.Attr("team_id", (int)client.Profile.RoomPlayer.TeamId);  //TODO в какой тиме свободно

                client.QueryGet(gameroom_offer, room.Channel.Jid);
            }

        }
    }
}
