using EmuWarface.Core;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Xml;

namespace EmuWarface.Game.Clans
{
    public static class Clan
    {
        public static Dictionary<ulong, int> ClanList = new Dictionary<ulong, int>();

        static Clan()
        {
            //GenerateClanList();

            var timer = new Timer(60000)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += GenerateClanList;
        }

        public static void GenerateClanList(object source, ElapsedEventArgs e) => GenerateClanList();
        public static void GenerateClanList()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var db_clan_members = SQL.QueryRead($"SELECT * FROM emu_clan_members");

            var list = new Dictionary<ulong, int>();

            foreach (DataRow row_member in db_clan_members.Rows)
            {
                ulong clan_id       = Convert.ToUInt64(row_member["clan_id"]);
                int member_points   = (int)row_member["clan_points"];

                if (!list.ContainsKey(clan_id))
                {
                    list.Add(clan_id, member_points);
                }
                else
                {
                    list[clan_id] += member_points;
                }
            }

            ClanList = list.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            sw.Stop();

            Log.Debug(string.Format("[Clan] Clan list generated in {0},{1:000}s.", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));
        }

        public static ulong GetClanId(ulong profile_id)
        {
            var db = SQL.QueryRead($"SELECT clan_id FROM emu_clan_members WHERE profile_id={profile_id}");

            if (db.Rows.Count != 1)
                return 0;

            return Convert.ToUInt64(db.Rows[0]["clan_id"]);
        }

        public static string GetClanName(ulong clan_id)
        {
            if(clan_id == 0)
                return string.Empty;

            var db = SQL.QueryRead($"SELECT name FROM emu_clans WHERE clan_id={clan_id}");
            if (db.Rows.Count != 1)
                return string.Empty;

            return (string)db.Rows[0]["name"];
        }

        public static IEnumerable<Client> GetOnlineClanMembers(ulong clan_id)
        {
            var online_members = new List<Client>();

            var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE clan_id={clan_id}");
            foreach (DataRow row in db.Rows)
            {
                var profile_id = Convert.ToUInt64(row["profile_id"]);
                lock (Server.Clients)
                {
                    var member = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id);
                    if (member != null)
                    {
                        online_members.Add(member);
                    }
                }
            }

            return online_members;
        }

        public static void AddMemberPoints(ulong clan_id, ulong profile_id, int points)
        {
            if (clan_id == 0)
                return;

            var clan_points = GetClanMemberPoints(profile_id, clan_id) + points;

            SQL.Query($"UPDATE emu_clan_members SET clan_points={clan_points} WHERE profile_id={profile_id} AND clan_id={clan_id}");

            ClanMembersUpdated(clan_id, profile_id);
        }

        //TODO переделать, сделать вызов каждому игроку конкретно
        public static void ClanMembersUpdated(ulong clan_id, params ulong[] updated_members)
        {
            if (clan_id == 0)
                return;

            XmlElement clan_members_updated = Xml.Element("clan_members_updated");

            foreach (ulong profile_id in updated_members)
            {
                XmlElement update_target = Xml.Element("update").Attr("profile_id", profile_id).Child(GetClanMemberInfo(profile_id, clan_id));
                clan_members_updated.Child(update_target);
            }

            foreach (var receiver in GetOnlineClanMembers(clan_id))
            {
                //if (ignore_members != null && ignore_members.Contains(receiver.Value))
                //    continue;
                receiver.Profile.Room?.GetExtension<GameRoomCore>().Update();

                //Iq iq = new Iq(IqType.Get, receiver.Jid, receiver.Channel.Jid);
                //receiver.QueryGet(iq.SetQuery(clan_members_updated));
                receiver.QueryGet(clan_members_updated);
            }
        }

        public static void ClanDescriptionUpdated(ulong clan_id, string description)
        {
            var receivers = GetOnlineClanMembers(clan_id);

            XmlElement clan_description_updated = Xml.Element("clan_description_updated")
                .Attr("description", description);

            foreach (var receiver in receivers)
            {
                //Iq iq = new Iq(IqType.Get, receiver.Jid, receiver.Channel.Jid);
                //receiver.QueryGet(iq.SetQuery(clan_description_updated));
                receiver.QueryGet(clan_description_updated);
            }
        }
        //clan_masterbanner_update
        public static void ClanMasterBannerUpdated(ulong clan_id, ulong master_id) => ClanMasterBannerUpdated(clan_id, Profile.GetProfile(master_id));
        public static void ClanMasterBannerUpdated(ulong clan_id, Profile master)
        {
            if (master == null)
                throw new InvalidOperationException();

            var receivers = GetOnlineClanMembers(clan_id);

            XmlElement clan_masterbanner_update = Xml.Element("clan_masterbanner_update")
                .Attr("master_badge", master.BannerBadge)
                .Attr("master_mark", master.BannerMark)
                .Attr("master_stripe", master.BannerStripe);

            foreach (var receiver in receivers)
            {
                //Iq iq = new Iq(IqType.Get, receiver.Jid, receiver.Channel.Jid);
                //receiver.QueryGet(iq.SetQuery(clan_masterbanner_update));
                receiver.QueryGet(clan_masterbanner_update);
            }
        }

        public static string GetClanLeader(ulong clan_id)
        {
            var db = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE clan_id='{clan_id}' AND clan_role='1'");

            if (db.Rows.Count == 1)
            {
                var profile_id = Convert.ToUInt64(db.Rows[0]["profile_id"]);
                return Profile.GetNickname(profile_id);
            }

            return string.Empty;
        }

        public static int GetClanPosition(ulong clan_id)
        {
            //TODO
            int position = 1;

            foreach (var clan in ClanList)
            {
                if (clan.Key == clan_id)
                    return position;

                position++;
            }

            return (int)clan_id;
        }

        public static int GetClanMemberPoints(ulong profile_id, ulong clan_id)
        {
            if (clan_id == 0)
                return 0;

            var db_clan_member = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={profile_id} AND clan_id={clan_id}");

            if (db_clan_member.Rows.Count == 0)
                return 0;

            return (int)db_clan_member.Rows[0]["clan_points"];
        }

        public static XmlElement GetClanMemberInfo(ulong profile_id, ulong clan_id)
        {
            if (clan_id == 0) 
                return null;

            var db_clan_member      = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE profile_id={profile_id} AND clan_id={clan_id}");
            var db_profile_member   = SQL.QueryRead($"SELECT * FROM emu_profiles WHERE profile_id={profile_id}");

            if (db_profile_member.Rows.Count == 0 || db_clan_member.Rows.Count == 0)
                return null;

            return Xml.Element("clan_member_info")
                .Attr("nickname", db_profile_member.Rows[0]["nickname"])
                .Attr("profile_id", db_profile_member.Rows[0]["profile_id"])
                .Attr("experience", db_profile_member.Rows[0]["experience"])
                .Attr("clan_points", db_clan_member.Rows[0]["clan_points"])
                .Attr("invite_date", db_clan_member.Rows[0]["invite_date"])
                .Attr("clan_role", db_clan_member.Rows[0]["clan_role"])
                .Attr("jid",    Profile.GetOnlineId(profile_id)) //TODO
                .Attr("status", (int)Profile.GetOnlineStatus(profile_id));
        }

        public static XmlElement ClanSerialize(ulong clan_id)
        {
            var db_clan = SQL.QueryRead($"SELECT * FROM emu_clans WHERE clan_id={clan_id}");
            var db_clan_members = SQL.QueryRead($"SELECT * FROM emu_clan_members WHERE clan_id={clan_id}");

            //TODO
            XmlElement clan = Xml.Element("clan")
                .Attr("name",           db_clan.Rows[0]["name"])
                .Attr("description",    db_clan.Rows[0]["description"])
                .Attr("clan_id",        db_clan.Rows[0]["clan_id"])
                .Attr("creation_date",  db_clan.Rows[0]["creation_date"])
                //.Attr("clan_points",    /*db_clan.Rows[0]["clan_points"]*/ 0)
                .Attr("leaderboard_position", GetClanPosition(clan_id));

            //int clan_points = 0;

            int clan_points = 0;

            foreach (DataRow db_clan_member in db_clan_members.Rows)
            {
                var db_profile_member = SQL.QueryRead($"SELECT * FROM emu_profiles WHERE profile_id={db_clan_member["profile_id"]}");

                if (db_profile_member.Rows.Count == 0)
                    continue;

                clan_points += (int)db_clan_member["clan_points"];

                //TODO статус и жид в одном
                XmlElement clan_member_info = Xml.Element("clan_member_info")
                    .Attr("nickname", db_profile_member.Rows[0]["nickname"])
                    .Attr("profile_id", db_profile_member.Rows[0]["profile_id"])
                    .Attr("experience", db_profile_member.Rows[0]["experience"])
                    .Attr("clan_points", db_clan_member["clan_points"])
                    .Attr("invite_date", db_clan_member["invite_date"])
                    .Attr("clan_role", db_clan_member["clan_role"])
                    .Attr("jid",    Profile.GetOnlineId(Convert.ToUInt64(db_clan_member["profile_id"])))
                    .Attr("status", (int)Profile.GetOnlineStatus(Convert.ToUInt64(db_clan_member["profile_id"])));

                if ((ClanRole)db_clan_member["clan_role"] == ClanRole.Master)
                {
                    clan.Attr("master_badge", db_profile_member.Rows[0]["banner_badge"])
                        .Attr("master_mark", db_profile_member.Rows[0]["banner_mark"])
                        .Attr("master_stripe", db_profile_member.Rows[0]["banner_stripe"]);
                }
                clan.Child(clan_member_info);
            }

            clan.Attr("clan_points", clan_points);

            /*
             <clan_member_info 
            nickname="Мерцание" 
            profile_id="64889" 
            experience="4228406" 
            clan_points="1166" 
            invite_date="1621967037" 
            clan_role="1" 
            jid=""
            status="0" />
             */

            //TODO
            //place_token place_info_token mode_info_token mission_info_token

            return clan;
        }

        public static void ClanInfo(Client receiver)
        {
            ClanInfo(receiver.Profile.ClanId, new Client[] { receiver });
        }

        public static void ClanInfo(ulong clan_id, IEnumerable<Client> receivers = null)
        {
            XmlElement clan_info = Xml.Element("clan_info");

            if (clan_id != 0)
                clan_info.Child(ClanSerialize(clan_id));

            //TODO temp убрать если все и так работает
            if (receivers == null)
            {
                receivers = GetOnlineClanMembers(clan_id);
                /*receivers = new List<Jid>();
                foreach(DataRow row in db_clan_members.Rows)
                {
                    var jid = Profile.GetOnlineId((ulong)row["profile_id"]);

                    if (jid != string.Empty)
                        receivers.Add(jid);
                }*/
            }

            foreach (var receiver in receivers)
            {
                //Iq iq = new Iq(IqType.Get, receiver.Jid, receiver.Channel.Jid);
                //receiver.QueryGet(iq.SetQuery(clan_info));
                receiver.QueryGet(clan_info);
            }
        }
    }
}
