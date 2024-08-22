using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public class Friend
    {
        /*
    <friend_list>
        <friend jid='747193555@warface/GameClient' profile_id='25785570' nickname='МрЛоки1965Герда' status='17' experience='0' location='В зале'/>
        <friend jid='' profile_id='25785570' nickname='МрЛоки1965Герда' status='2' experience='0' location=''/>
    </friend_list>
         */
        public static void RemoveFriend(ulong profile_id, ulong friend_id)
        {
            SQL.Query($"DELETE FROM emu_friends WHERE first_id={profile_id} AND second_id={friend_id}");
            SQL.Query($"DELETE FROM emu_friends WHERE first_id={friend_id} AND second_id={profile_id}");
        }

        public static void AddFriend(ulong profile_id, ulong friend_id)
        {
            SQL.Query($"INSERT INTO emu_friends (`first_id`, `second_id`) VALUES ('{profile_id}', '{friend_id}')");
            SQL.Query($"INSERT INTO emu_friends (`first_id`, `second_id`) VALUES ('{friend_id}', '{profile_id}')");
        }

        public static List<ulong> GetFriends(ulong profile_id)
        {
            if (profile_id == 0)
                throw new InvalidOperationException();

            List<ulong> friends = new List<ulong>();

            var result = SQL.QueryRead($"SELECT * FROM emu_friends WHERE first_id={profile_id}").Rows;
            foreach (DataRow row in result)
            {
                friends.Add((ulong)row["second_id"]);
            }

            return friends;
        }

        public static void FriendList(Client client)
        {
            if (client.Profile == null || client.Channel == null) 
                throw new InvalidOperationException();

            List<ulong> friends = GetFriends(client.ProfileId);

            XmlElement response = Xml.Element("friend_list");
            foreach (var friend_id in friends)
            {
                var friendProfile = Profile.GetProfile(friend_id);

                if(friendProfile != null)
                {
                    var friend = Xml.Element("friend")
                        .Attr("jid",        Profile.GetOnlineId(friendProfile.Id))//TODO TEMP
                        .Attr("profile_id", friendProfile.Id)
                        .Attr("nickname",   friendProfile.Nickname)
                        .Attr("status",     (int)Profile.GetOnlineStatus(friendProfile.Id))
                        .Attr("experience", friendProfile.Experience)
                        .Attr("location", "");
                    response.Child(friend);
                }
            }

            //Iq iq = new Iq(IqType.Get, client.Jid, client.Channel.Jid);
            //client.QueryGet(iq.SetQuery(response));
            client.QueryGet(response);
        }
    }
}
