using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Enums.Errors;

using EmuWarface.Game.Items;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class CreateProfile
    {
        [Query(IqType.Get, "create_profile")]
        public static void CreateProfileSerializer(Client client, Iq iq)
        {
            string version = iq.Query.GetAttribute("version");
            string head = iq.Query.GetAttribute("head");
            string resource = iq.Query.GetAttribute("resource");
            string nickname = iq.Query.GetAttribute("nickname");

            if (!GameData.ValidateInputString("Nickname", nickname) || nickname.Length < 4 || nickname.Length > 16)
                throw new QueryException(CreateProfileError.InvalidNickname);

            MySqlCommand cmd = new MySqlCommand($"SELECT nickname FROM emu_profiles WHERE nickname=@nickname");
            cmd.Parameters.AddWithValue("@nickname", nickname);
            var db = SQL.QueryRead(cmd);

            if (db.Rows.Count != 0)
                throw new QueryException(CreateProfileError.ReservedNickname);

            MasterServer channel = Server.Channels.FirstOrDefault(x => x.Resource == resource);

            if (channel == null ||
                client.Channel != null)
                throw new InvalidOperationException();

#if !DEBUG
            if (version != EmuConfig.Settings.GameVersion)
                throw new QueryException(JoinChannelError.VersionMismatch);
#endif

            //if (channel.MinRank > client.Profile.GetRank() || channel.MaxRank < client.Profile.GetRank())
            //    throw new QueryException(CreateProfileError);

            if (client.Profile != null)
                throw new QueryException(CreateProfileError.AlreadyExist);

            /*client.Profile = */
            Profile.Create(client.UserId, head, nickname);

            XmlElement create_profile = Xml.Element(iq.Query.LocalName).Attr("profile_id", client.ProfileId);
            XmlElement character = client.Profile.CharacterSerialize();

            //TODO баны
            character.Child(Xml.Element("ProfileBans"));

            //Item.GetExpiredItems(client.ProfileId, client.Profile.Items)
            //    .ForEach(expired_item => character.Child(expired_item));

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

            var db_login_bonus = SQL.QueryRead($"SELECT * FROM emu_login_bonus WHERE profile_id={client.ProfileId}").Rows[0];

            character.Child(Xml.Element("login_bonus")
                .Attr("current_streak", db_login_bonus["current_streak"])
                .Attr("current_reward", db_login_bonus["current_reward"]));

            Notification.GetNotifications(client.ProfileId).ForEach(x => character.Child(x.Serialize()));

            character.Child(GameData.ClVariables);

            create_profile.Child(character);

            //sw.Stop();

            //Log.Info(String.Format("{0:n0} kb", (GC.GetTotalMemory(false) - initMemoryUsage) / 1024));

            client.Channel = channel;
            client.Presence = PlayerStatus.Online | PlayerStatus.InLobby;

            //Friend.FriendList(client);
            //Clan.ClanInfo(client);

            //client.Send("<iq to='3@warface/GameClient' from='masterserver@warface/pvp_skilled_001' type='get' id='uid00000005'><query xmlns='urn:cryonline:k01'><friend_list xmlns=''/></query></iq>");

            //client.Presence = PlayerStatus.Online | PlayerStatus.InLobby;

            iq.SetQuery(create_profile);
            iq.To = channel.Jid;

            client.QueryResult(iq);
        }
    }
}
