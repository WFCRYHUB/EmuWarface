using EmuWarface.Core;
using EmuWarface.Game.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EmuWarface.Game
{
    public class ProfileBan
    {
        public ulong ProfileId;
        public RoomType RoomType;
        public BanType  BanType;
        public long BanSecondsLeft;
        public long TrialSecondsLeft;
        public int LastBanIndex;

        public static List<ProfileBan> GetBans(ulong profile_id)
        {
            List<ProfileBan> items = new List<ProfileBan>();

            var db = SQL.QueryRead($"SELECT * FROM emu_profile_bans WHERE profile_id={profile_id}");

            /*foreach (DataRow row in db.Rows)
            {
                items.Add(ParseDataRow(row));
            }*/

            return items;
        }

        /*private static ProfileBan ParseDataRow(DataRow row)
        {
            return new ProfileBan
            {
                ProfileId   = (ulong)row["profile_id"],
                RoomType    = (ulong)row["profile_id"],
                BanType     = (ulong)row["profile_id"],
            };
        }*/
    }
}
