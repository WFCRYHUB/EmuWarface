using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class CheckNickname
    {
        [Query(IqType.Get, "check_nickname")]
        public static void CheckNicknameSerializer(Client client, Iq iq)
        {
            var q = iq.Query;

            var nickname = q.GetAttribute("nickname");

            if (!GameData.ValidateInputString("Nickname", nickname) || nickname.Length < 4 || nickname.Length > 16)
            {
                q.SetAttribute("result", "1");
            }
            else
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT nickname FROM emu_profiles WHERE nickname=@nickname");
                cmd.Parameters.AddWithValue("@nickname", nickname);
                var db = SQL.QueryRead(cmd);

                if (db.Rows.Count == 0)
                {
                    q.SetAttribute("result", "0");
                }
                else
                {
                    q.SetAttribute("result", "2");
                }
            }

            iq.To = Server.Channels.First().Jid;
            client.QueryResult(iq);
        }
    }
}

