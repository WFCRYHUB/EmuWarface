using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Enums.Errors;

using EmuWarface.Game.Items;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class JoinChannel
    {
        [Query(IqType.Get, "join_channel")]
        public static void JoinChannelSerializer(Client client, Iq iq)
        {
            ulong profile_id = ulong.Parse(iq.Query.GetAttribute("profile_id"));
            string version = iq.Query.GetAttribute("version");
            string resource = iq.Query.GetAttribute("resource");

            MasterServer channel = Server.Channels.FirstOrDefault(x => x.Resource == resource);

            if (channel == null ||
                client.Channel != null ||
                client.Profile == null ||
                client.ProfileId != profile_id)
                throw new ServerException("profile not created");

#if !DEBUGLOCAL
            if (version != EmuConfig.Settings.GameVersion)
                throw new QueryException(JoinChannelError.VersionMismatch);
#endif
            if (channel.MinRank > client.Profile.GetRank() || channel.MaxRank < client.Profile.GetRank())
                throw new QueryException(JoinChannelError.RankRestricted);

            XmlElement join_channel = Xml.Element(iq.Query.LocalName);
            XmlElement character = client.Profile.CharacterSerialize();

            character.Child(Xml.Element("ProfileBans"));

            Item.GetExpiredItems(profile_id, client.Profile.Items)
                .ForEach(expired_item => character.Child(expired_item));

            client.Profile.Items.ForEach(item => character.Child(item.Serialize()));

            //TODO sponsors
            XmlElement sponsor_info = Xml.Element("sponsor_info");
            sponsor_info.Child(Xml.Element("sponsor").Attr("sponsor_id", "0").Attr("sponsor_points", "0").Attr("next_unlock_item", ""));
            sponsor_info.Child(Xml.Element("sponsor").Attr("sponsor_id", "1").Attr("sponsor_points", "0").Attr("next_unlock_item", ""));
            sponsor_info.Child(Xml.Element("sponsor").Attr("sponsor_id", "2").Attr("sponsor_points", "0").Attr("next_unlock_item", ""));
            character.Child(sponsor_info);

            character.Child(Xml.Element("chat_channels").Child(Xml.Element("chat").Attr("channel", "0").Attr("channel_id", "global." + channel.Resource).Attr("service_id", "conference.warface")));

            character.Child(client.Profile.ProgressionSerialize());

            //TODO daily bonus
            client.Profile.CheckDailyBonus();

            var db_login_bonus = SQL.QueryRead($"SELECT * FROM emu_login_bonus WHERE profile_id={profile_id}").Rows[0];

            character.Child(Xml.Element("login_bonus")
                .Attr("current_streak", db_login_bonus["current_streak"])
                .Attr("current_reward", db_login_bonus["current_reward"]));

            Notification.GetNotifications(profile_id).ForEach(x => character.Child(x.Serialize()));

            character.Child(GameData.ClVariables);

            join_channel.Child(character);

            //sw.Stop();

            //Log.Info(String.Format("{0:n0} kb", (GC.GetTotalMemory(false) - initMemoryUsage) / 1024));

            client.Channel = channel;
            client.Presence = PlayerStatus.Online | PlayerStatus.InLobby;

            EmuExtensions.UpdateOnline();

            Friend.FriendList(client);
            Clan.ClanInfo(client);

            //client.Send("<iq to='3@warface/GameClient' from='masterserver@warface/pvp_skilled_001' type='get' id='uid00000005'><query xmlns='urn:cryonline:k01'><friend_list xmlns=''/></query></iq>");

            //client.Presence = PlayerStatus.Online | PlayerStatus.InLobby;

            iq.SetQuery(join_channel);
            iq.To = channel.Jid;

            client.QueryResult(iq);
        }
    }
}
