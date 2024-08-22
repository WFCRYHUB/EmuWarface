using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Enums.Errors;

using EmuWarface.Game.Items;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class SwitchChannel
    {
        //<switch_channel version='1.18800.1734.41200' token=' ' profile_id='17352061' user_id='747973103'
        //region_id='global' resource='pve_059_r3' build_type='--release'/>

        [Query(IqType.Get, "switch_channel")]
        public static void SwitchChannelSerializer(Client client, Iq iq)
        {
            string version = iq.Query.GetAttribute("version");
            string resource = iq.Query.GetAttribute("resource");

            MasterServer channel = Server.Channels.FirstOrDefault(x => x.Resource == resource);

            if (channel == null ||
                client.Channel == null ||
                client.Profile == null)
                throw new InvalidOperationException();

#if !DEBUG
            if (version != EmuConfig.Settings.GameVersion)
                throw new QueryException(JoinChannelError.VersionMismatch);
#endif

            if (channel.MinRank > client.Profile.GetRank() || channel.MaxRank < client.Profile.GetRank())
                throw new QueryException(JoinChannelError.RankRestricted);

            XmlElement switch_channel = Xml.Element(iq.Query.LocalName);
            XmlElement character = client.Profile.CharacterSerialize();

            //TODO баны

            /*var profileBans = Xml.Element("ProfileBans");

            var ban = Xml.Element("ProfileBan")
                 .Attr("room_type", "PvP_Rating")
                 .Attr("ban_type", "Progressive")
                 .Attr("ban_seconds_left", "1537")
                 .Attr("trial_seconds_left", "3337")
                 .Attr("last_ban_index", "0");

            profileBans.Child(ban);

            character.Child(profileBans);*/

            //TODO test
            Item.GetExpiredItems(client.ProfileId, client.Profile.Items)
                .ForEach(expired_item => character.Child(expired_item));

            //var notif = Notification.LeaveGameBanNotification();
            // Notification.AddNotification(client.ProfileId, notif);

            Notification.GetNotifications(client.ProfileId)
                .ForEach(x => character.Child(x.Serialize()));


            character.Child(Xml.Element("chat_channels").Child(Xml.Element("chat").Attr("channel", "0").Attr("channel_id", "global." + channel.Resource).Attr("service_id", "conference.warface")));
            character.Child(GameData.ClVariables);


            switch_channel.Child(character);

            client.Presence = PlayerStatus.Online | PlayerStatus.InLobby;

            Utils.UpdateOnline();

            client.Channel = channel;

            iq.SetQuery(switch_channel);
            iq.To = client.Channel.Jid;

            client.QueryResult(iq);
        }
    }
}
