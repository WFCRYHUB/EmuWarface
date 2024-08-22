using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class SetBanner
    {
        [Query(IqType.Get, "set_banner")]
        public static void SetBannerSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var achievs = Achievement.GetAchievements(client.ProfileId);

            uint banner_badge = uint.Parse(iq.Query.GetAttribute("banner_badge"));
            uint banner_mark = uint.Parse(iq.Query.GetAttribute("banner_mark"));
            uint banner_stripe = uint.Parse(iq.Query.GetAttribute("banner_stripe"));

            if (banner_badge == uint.MaxValue || banner_badge == uint.MinValue)
                client.Profile.BannerBadge = banner_badge;

            if (banner_mark == uint.MaxValue || banner_mark == uint.MinValue)
                client.Profile.BannerMark = banner_mark;

            if (banner_stripe == uint.MaxValue || banner_stripe == uint.MinValue)
                client.Profile.BannerStripe = banner_stripe;

            foreach (var achiev in achievs)
            {
                if (achiev.AchievementId == banner_badge && achiev.IsCompleted)
                    client.Profile.BannerBadge = banner_badge;

                if (achiev.AchievementId == banner_mark && achiev.IsCompleted)
                    client.Profile.BannerMark = banner_mark;

                if (achiev.AchievementId == banner_stripe && achiev.IsCompleted)
                    client.Profile.BannerStripe = banner_stripe;
            }

            SQL.Query($"UPDATE emu_profiles SET banner_badge={client.Profile.BannerBadge}, banner_mark={client.Profile.BannerMark}, banner_stripe={client.Profile.BannerStripe} WHERE profile_id={client.ProfileId}");

            iq.SetQuery(Xml.Element("set_banner"));

            client.Profile.Room?.GetExtension<GameRoomCore>()?.Update();

            //Clan.ClanMasterBannerUpdated(clan_id, new_master_id);

            //return iq.SetQuery(Xml.Element("set_banner"));


            client.QueryResult(iq);
        }
    }
}
