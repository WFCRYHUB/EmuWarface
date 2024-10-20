using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class ClanCreate
    {
        [Query(IqType.Get, "clan_create")]
        public static void ClanCreateSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            string clan_name = iq.Query.GetAttribute("clan_name");
            string description = iq.Query.GetAttribute("description");

            if (!GameData.ValidateInputString("Clanname", clan_name) || clan_name.Length < 4 || clan_name.Length > 16)
                throw new QueryException(ClanCreationStatus.InvalidName);

            //TODO максимум символов в описании?
            var desc = Utils.Base64Decode(description);
            if (!GameData.ValidateInputString("Clandesc", desc) || desc.Length > 1000)
                throw new QueryException(ClanCreationStatus.ServiceError);

            //TODO check clan name and description
            //TODO покупка предмета создания клана

            var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={client.ProfileId}");

            if (db.Rows.Count != 0)
                throw new QueryException(ClanCreationStatus.AlreadyClanMember);

            //if (client.Profile.Experience < 18800)
            //    throw new QueryException(ClanCreationStatus.NeedBuyItem);

            /*if (SQL.QueryRead($"SELECT * FROM emu_clans WHERE name={clan_name}").Rows.Count != 0) 
                return iq.Error((int)ClanCreationStatus.DuplicateName);*/

            MySqlCommand cmd = new MySqlCommand($"INSERT INTO emu_clans (`name`, `description`, `creation_date`) VALUES (@name, @description, @creation_date); SELECT LAST_INSERT_ID();");
            cmd.Parameters.AddWithValue("@name", clan_name);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@creation_date", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            ulong clan_id = 0;
            try
            {
                var db_clan = SQL.QueryRead(cmd);
                clan_id = Convert.ToUInt64(db_clan.Rows[0][0]);
            }
            catch (Exception e)
            {
                string exception = e.ToString();

                if (exception.Contains("Duplicate"))
                    throw new QueryException(ClanCreationStatus.DuplicateName);

                Log.Error(e.ToString());
                return;
            }

            if (clan_id == 0) throw new QueryException(1);

            SQL.Query($"INSERT INTO emu_clan_members (`profile_id`, `clan_id`, `clan_role`, `invite_date`) VALUES ({client.ProfileId}, {clan_id}, {(int)ClanRole.Master}, {DateTimeOffset.UtcNow.ToUnixTimeSeconds()})");

            XmlElement clan_create = Xml.Element("clan_create");
            clan_create.Child(Clan.ClanSerialize(clan_id));

            client.Profile.Room?.GetExtension<GameRoomCore>()?.Update();

            iq.SetQuery(clan_create);
            client.QueryResult(iq);
        }
    }
}
