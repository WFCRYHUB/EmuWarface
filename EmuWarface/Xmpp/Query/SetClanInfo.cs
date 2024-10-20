using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;

namespace EmuWarface.Xmpp.Query
{
    public static class SetClanInfo
    {
        [Query(IqType.Get, "set_clan_info")]
        public static void SetClanInfoSerializer(Client client, Iq iq)
        {
            if (client.Profile == null || client.Profile.ClanId == 0)
                throw new InvalidOperationException();

            var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={client.ProfileId}");

            if (db.Rows.Count == 0)
                throw new QueryException(1);

            ulong clan_id = Convert.ToUInt64(db.Rows[0]["clan_id"]);
            ClanRole clan_role = (ClanRole)db.Rows[0]["clan_role"];

            if (clan_id == 0 || clan_role != ClanRole.Master)
                throw new QueryException(1);

            //TODO
            //чек маты и т.д.

            string description = iq.Query.GetAttribute("description");

            var desc = Utils.Base64Decode(description);
            if (!GameData.ValidateInputString("Clandesc", desc) || desc.Length > 1000)
                throw new QueryException(1);

            MySqlCommand cmd = new MySqlCommand("UPDATE emu_clans SET description=@description WHERE clan_id=@clan_id");
            cmd.Parameters.AddWithValue("@clan_id", clan_id);
            cmd.Parameters.AddWithValue("@description", description);
            SQL.Query(cmd);

            //XmlElement clan_description_updated = Xml.Element("clan_description_updated")
            //    .Attr("description", description);
            //clan_description_updated

            Clan.ClanDescriptionUpdated(clan_id, description);
            client.QueryResult(iq);
        }
    }
}

