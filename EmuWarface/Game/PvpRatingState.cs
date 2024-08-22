using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public class PvpRatingState
    {
        public ulong    ProfileId       { get; set; }
        public int      Rank            { get; set; }
        public int      MaxRank         { get; set; }
        public string   GamesHistory    { get; set; }

        public static PvpRatingState GetPvpRatingState(ulong profile_id)
        {
            var db = SQL.QueryRead($"SELECT * FROM emu_pvp_rating WHERE profile_id={profile_id}");

            if (db.Rows.Count == 0)
                return null;

            var ratingState = ParseDataRow(db.Rows[0]);
            ratingState.ProfileId = profile_id;

            return ratingState;
        }

        public void Update()
        {
            SQL.Query($"UPDATE emu_pvp_rating SET games_history='{GamesHistory}', rank='{Rank}', max_rank='{MaxRank}' WHERE profile_id={ProfileId}");
        }

        public void UpdateRating(bool win)
        {
            var rating_curve = QueryCache.GetCache("get_configs").Data["rating_curve"];

            XmlElement rating = rating_curve.GetElementsByTagName("rating")[Rank] as XmlElement;

            var games_count     = int.Parse(rating.GetAttribute("games_count"));
            var games_to_win    = int.Parse(rating.GetAttribute("games_to_win"));

            GamesHistory += win ? "w" : "f";

            var player_games = GamesHistory.Length;
            var player_wins = GamesHistory.Count(x => x == 'w');

            if (player_wins >= games_to_win)
            {
                if (Rank != 21)
                    Rank += 1;

                if (Rank > MaxRank)
                {
                    MaxRank = Rank;

                    Profile profile = Profile.GetProfile(ProfileId);
                    if (profile != null)
                    {
                        profile.GiveRatingBonus(Rank);
                    }
                }

                GamesHistory = string.Empty;
            }

            if (games_count - player_games < games_to_win)
            {
                if (Rank > 0)
                    Rank -= 1;

                GamesHistory = string.Empty;
            }

            Update();
        }

        private static PvpRatingState ParseDataRow(DataRow row)
        {
            return new PvpRatingState
            {
                Rank            = (int)row["rank"],
                MaxRank         = (int)row["max_rank"],
                GamesHistory    = (string)row["games_history"]
            };
        }
    }
}
