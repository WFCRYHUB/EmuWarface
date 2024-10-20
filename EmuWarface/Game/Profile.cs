using EmuWarface.Core;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Items;
using EmuWarface.Game.Notifications;
using EmuWarface.Game.Shops;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public class Profile
    {
        public ulong Id             { get; private set; }
        public string Head          { get; private set; }
        public string Nickname      { get; set; }
        public int Height           { get; set; }
        public int Fatness          { get; set; }
        public uint BannerBadge     { get; set; }
        public uint BannerMark      { get; set; }
        public uint BannerStripe    { get; set; }
        public int GameMoney        { get; set; }
        public int CryMoney         { get; set; }
        public int CrownMoney       { get; set; }
        public ClassId CurrentClass { get; set; }
        public bool IsExperienceFreezed { get; set; }
        public RoomPlayerInfo RoomPlayer   { get; set; }
        public GameRoom Room => RoomPlayer?.Room;

        //TODO get сделать если null то SELECT в бд
        //private ulong?              _clanId;
        private PvpRatingState      _pvpRatingState;
        private List<Item>          _items { get; set; }
        private List<PlayerStat>    _stats;
        private List<Achievement>   _achievements;

        private int _experience;
        public int Experience
        {
            get
            {
                return _experience;
            }
            set
            {
                _experience = Math.Clamp(value, 0, 23046000);
            }
        }

        private int _rank;
        public int GetRank()
        {
            var rank = GameData.GetRank(Experience);

            if (_rank == 0)
            {
                _rank = rank;
            }
            else if (rank > _rank)
            {
                //TODO если ранк изменился выдавать награду
                Notification.SyncNotifications(Id, Notification.NewRankReachedNotification(_rank, rank));

                _rank = rank;
            }

            return _rank;
        }

        public ulong ClanId
        {
            get
            {
                //if (_clanId == null)
                //    _clanId = Clan.GetClanId(Id);

                //return (ulong)_clanId;
                return Clan.GetClanId(Id);
            }
        }

        public string ClanName
        {
            get
            {
                if(ClanId != 0)
                    return Clan.GetClanName(ClanId);

                return "";
            }
        }

        public PvpRatingState PvpRatingState
        {
            get
            {
                if (_pvpRatingState == null)
                    _pvpRatingState = PvpRatingState.GetPvpRatingState(Id);

                return _pvpRatingState;
            }
        }

        public List<Item> Items
        {
            get
            {
                if (_items == null)
                    _items = Item.GetItems(Id);

                lock (_items)
                {
                    _items.RemoveAll(x => x == null);
                }

                return _items;
            }
        }
        public List<Notification> Notifications
        {
            get
            {
                return Notification.GetNotifications(Id);
            }
        }
        public List<PlayerStat> Stats
        {
            get
            {
                if (_stats == null)
                    _stats = PlayerStat.GetPlayerStats(Id);

                return _stats;
            }
        }
        public List<Achievement> Achievements
        {
            get
            {
                if (_achievements == null)
                    _achievements = Achievement.GetAchievements(Id);

                return _achievements;
            }
        }

        public int CheckRankUpdated() => GetRank();

        public void CheckDailyBonus()
        {
            //Notification.AddNotification(Id, Notification.LeaveGameBanNotification());
            //Notification.SyncNotifications(Id, Notification.LeaveGameBanNotification());

            DataTable db = SQL.QueryRead($"SELECT * FROM emu_login_bonus WHERE profile_id={Id}");

            sbyte current_streak = 0;
            sbyte current_reward = -1;
            long last_seen_reward = 0;

            if (db.Rows.Count != 1)
            {
                SQL.Query($"INSERT INTO emu_login_bonus (`profile_id`, `last_seen_reward`) VALUES ('{Id}', '{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}');");
            }
            else 
            {
                current_streak = (sbyte)db.Rows[0]["current_streak"];
                current_reward = (sbyte)db.Rows[0]["current_reward"];
                last_seen_reward = (long)db.Rows[0]["last_seen_reward"];
            }

            var last = DateTimeOffset.FromUnixTimeSeconds(last_seen_reward).Add(TimeSpan.FromHours(3)).DayOfYear;
            var now = DateTime.UtcNow.Add(TimeSpan.FromHours(3)).DayOfYear;

            if (now - last != 0)
            {
                GiveItem("mission_access_token_04", ItemType.Consumable, quantity: 3);
                var access_notif = Notification.GiveItemNotification("mission_access_token_04", "Consumable", true, 3600, 3);
                Notification.AddNotification(Id, access_notif);
                Notification.SyncNotifications(Id, access_notif);

                if (now - last == 1)
                    current_reward += 1;

                if (now - last > 1)
                    current_reward = 0;

                var config = QueryCache.GetCache("get_configs").Data;

                var streak = config["consecutive_login_bonus"].ChildNodes[current_streak];
                if (streak == null)
                    return;

                var event_name = (XmlElement)streak.ChildNodes[current_reward];
                if (event_name == null)
                    return;

                foreach(XmlElement @event in config["special_reward_configuration"].ChildNodes)
                {
                    if(@event.GetAttribute("name") == event_name.GetAttribute("name"))
                    {
                        foreach (XmlElement reward in @event.ChildNodes)
                        {
                            Notification notif = null;

                            switch (reward.Name)
                            {
                                case "money":
                                    {
                                        string currency = reward.GetAttribute("currency");
                                        int amount = int.Parse(reward.GetAttribute("amount"));

                                        switch (currency)
                                        {
                                            case "cry_money":
                                                GameMoney += amount;
                                                break;
                                            case "crown_money":
                                                CrownMoney += amount;
                                                break;
                                            case "game_money":
                                                CryMoney += amount;
                                                break;
                                        }

                                        notif = Notification.GiveMoneyNotification(currency, amount, true);
                                    }
                                    break;
                                case "item":
                                    {
                                        var name = reward.GetAttribute("name");

                                        if (name.Contains("coin"))
                                            break;

                                        if (name.Contains("box"))
                                        {
                                            notif = GiveRandomBox(name);
                                        }
                                        else
                                        {
                                            ItemType type = ItemType.Permanent;
                                            long seconds = 0;
                                            int amount = 0;

                                            if (reward.GetAttribute("expiration") != null)
                                            {
                                                seconds = Utils.GetTotalSeconds(reward.GetAttribute("expiration"));
                                                type = ItemType.Expiration;
                                            }
                                            else if (reward.GetAttribute("amount") != null)
                                            {
                                                amount = int.Parse(reward.GetAttribute("amount"));
                                                type = ItemType.Consumable;
                                            }

                                            GiveItem(name, type, seconds, amount);
                                            notif = Notification.GiveItemNotification(name, type.ToString(), true, seconds, amount);
                                        }
                                    }
                                    break;
                            }

                            if (notif != null)
                            {
                                var bonus = Xml.Element("consecutive_login_bonus")
                                    .Attr("previous_streak", current_streak - 1)
                                    .Attr("previous_reward", current_reward - 1)
                                    .Attr("current_streak", current_streak)
                                    .Attr("current_reward", current_reward);
                                notif.Element.Child(bonus);

                                Notification.AddNotification(Id, notif);
                                Notification.SyncNotifications(Id, notif);

                                Update();
                            }
                        }
                    }
                }
            }

            SQL.Query($"UPDATE emu_login_bonus SET current_streak='{current_streak}', current_reward='{current_reward}', last_seen_reward='{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}' WHERE profile_id={Id};");
        }

        public void GiveRatingBonus(int rank)
        {
            var config = QueryCache.GetCache("get_configs").Data;

            foreach (XmlElement @event in config["special_reward_configuration"].ChildNodes)
            {
                if (@event.GetAttribute("name") == "rating_level_" + rank + "_achieved")
                {
                    foreach (XmlElement reward in @event.ChildNodes)
                    {
                        Notification notif = null;

                        switch (reward.Name)
                        {
                            case "money":
                                {
                                    string currency = reward.GetAttribute("currency");
                                    int amount = int.Parse(reward.GetAttribute("amount"));

                                    switch (currency)
                                    {
                                        case "cry_money":
                                            GameMoney += amount;
                                            break;
                                        case "crown_money":
                                            CrownMoney += amount;
                                            break;
                                        case "game_money":
                                            CryMoney += amount;
                                            break;
                                    }

                                    notif = Notification.GiveMoneyNotification(currency, amount, true);
                                }
                                break;
                            case "item":
                                {
                                    var name = reward.GetAttribute("name");

                                    if (name.Contains("coin"))
                                        break;

                                    if (name.Contains("box"))
                                    {
                                        notif = GiveRandomBox(name);
                                    }
                                    else
                                    {
                                        ItemType type = ItemType.Permanent;
                                        long seconds = 0;
                                        int amount = 0;

                                        if (reward.GetAttribute("expiration") != null)
                                        {
                                            seconds = Utils.GetTotalSeconds(reward.GetAttribute("expiration"));
                                            type = ItemType.Expiration;
                                        }
                                        else if (reward.GetAttribute("amount") != null)
                                        {
                                            amount = int.Parse(reward.GetAttribute("amount"));
                                            type = ItemType.Consumable;
                                        }

                                        GiveItem(name, type, seconds, amount);
                                        notif = Notification.GiveItemNotification(name, type.ToString(), true, seconds, amount);
                                    }
                                }
                                break;
                        }

                        if (notif != null)
                        {
                            Notification.AddNotification(Id, notif);
                            Notification.SyncNotifications(Id, notif);

                            Update();
                        }
                    }
                }
            }
        }

        public void GiveRandomBoxCards()
        {
            //var items = QueryCache.GetCache("items");
            var card_boxes = new List<string>();

            foreach (XmlElement item in GameData.RandomBoxCards.ChildNodes)
            {
                card_boxes.Add(item.GetAttribute("name"));
            }

            var rand_card_box = card_boxes[new Random().Next(0, card_boxes.Count)];
            var notif = GiveRandomBox(rand_card_box);

            Notification.AddNotification(Id, notif);
            lock (Server.Clients)
            {
                Notification.SyncNotifications(Server.Clients.FirstOrDefault(x => x.ProfileId == Id), notif);
            }
        }

        public Notification GiveRandomBox(string box_name)
        {
            XmlElement purchased_item = Xml.Element("purchased_item");

            var random_box = Shop.ShopItems.FirstOrDefault(x => x.GetAttribute("name") == box_name)?["random_box"];
            if (random_box == null)
                return null;

            Log.Info(box_name);

            var error = RandomBox.Open(this, random_box, ref purchased_item);

            if (error != ShopErrorCode.OK)
                return null;

            return Notification.GiveRandomBoxNotification(box_name, purchased_item);
        }
        public XmlElement CharacterSerialize()
        {
            return Xml.Element("character")
                .Attr("nick",           Nickname)
                .Attr("gender",         "male")
                .Attr("height",         Height)
                .Attr("fatness",        Fatness)
                .Attr("head",           Head)
                .Attr("current_class",  (int)CurrentClass)
                .Attr("experience",     Experience)
                .Attr("pvp_rating_points", "0")
                .Attr("pvp_rating_rank", PvpRatingState.Rank)
                .Attr("pvp_rating_games_history", PvpRatingState.GamesHistory)
                .Attr("banner_badge",   BannerBadge)
                .Attr("banner_mark",    BannerMark)
                .Attr("banner_stripe",  BannerStripe)
                .Attr("game_money",     GameMoney)
                .Attr("cry_money",      CryMoney)
                .Attr("crown_money",    CrownMoney);
        }

        public XmlElement ProgressionSerialize()
        {
            return Xml.Element("profile_progression_state")
                .Attr("profile_id", Id)
                .Attr("mission_unlocked", "trainingmission,easymission,normalmission,hardmission,zombieeasy,zombienormal,zombiehard,survivalmission,campaignsections,campaignsection1,campaignsection2,campaignsection3,volcanoeasy,volcanonormal,volcanohard,volcanosurvival,anubiseasy,anubisnormal,anubishard,anubiseasy2,anubisnormal2,anubishard2,zombietowereasy,zombietowernormal,zombietowerhard,icebreakereasy,icebreakernormal,icebreakerhard,chernobyleasy,chernobylnormal,chernobylhard,japaneasy,japannormal,japanhard,marseasy,marsnormal,marshard,blackwood,pve_arena")
                .Attr("tutorial_unlocked", "1")
                .Attr("tutorial_passed", "1")
                .Attr("class_unlocked", "31");
        }

        public XmlElement ResyncProfie()
        {
            XmlElement resync_profile = Xml.Element("resync_profile");

            _items = Item.GetItems(Id);

            lock (_items)
            {
                foreach (var item in _items)
                {
                    resync_profile.Child(item.Serialize());
                }
            }

            var money = Xml.Element("money")
                .Attr("cry_money", CryMoney)
                .Attr("crown_money", CrownMoney)
                .Attr("game_money", GameMoney);
            resync_profile.Child(money);

            resync_profile.Child(CharacterSerialize());

            resync_profile.Child(Xml.Element("progression").Child(ProgressionSerialize()));

            //TODO rating ban

            return resync_profile;
        }

        public XmlElement UpdateCryMoney()
        {
            XmlElement update_cry_money = Xml.Element("update_cry_money")
                .Attr("cry_money", CryMoney);

            return update_cry_money;
        }

        public void Update()
        {
            //UPDATE `emuwarface`.`emu_profiles` SET `experience`='1038495' WHERE  `profile_id`=3;
            SQL.Query($"UPDATE emu_profiles SET experience={Experience}, current_class={(int)CurrentClass}, height={Height}, fatness={Fatness}, banner_badge={BannerBadge}, banner_mark={BannerMark}, banner_stripe={BannerStripe}, game_money={GameMoney}, cry_money={CryMoney}, crown_money={CrownMoney}, last_seen_date={DateTimeOffset.UtcNow.ToUnixTimeSeconds()} WHERE profile_id={Id}");
        }

        public static void Create(ulong user_id, string head, string nickname)
        {
            //TODO
            //check nickname
            //проверять голову
            //проверять ник

            if(user_id == 0)
                throw new InvalidOperationException();

            if (string.IsNullOrEmpty(nickname))
                throw new InvalidOperationException();

            MySqlCommand cmd1 = new MySqlCommand("INSERT INTO emu_profiles (`user_id`, `nickname`, `head`, `last_seen_date`) VALUES (@user_id, @nickname, @head, @last_seen_date); SELECT LAST_INSERT_ID();");
            cmd1.Parameters.AddWithValue("@user_id", user_id);
            cmd1.Parameters.AddWithValue("@nickname", nickname);
            cmd1.Parameters.AddWithValue("@head", head);
            cmd1.Parameters.AddWithValue("@last_seen_date", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            ulong profile_id = 0;
            try
            {
                var res2 = SQL.QueryRead(cmd1);

                if (res2 == null)
                    throw new QueryException(CreateProfileError.ReservedNickname);

                profile_id = Convert.ToUInt64(res2.Rows[0][0]);
            }
            catch
            {
                throw new QueryException(CreateProfileError.ReservedNickname);
            }

            SQL.Query($"INSERT INTO emu_profile_progression_state (`profile_id`) VALUES ({profile_id});");
            SQL.Query($"INSERT INTO emu_pvp_rating (`profile_id`) VALUES ({profile_id});");
            SQL.Query($"INSERT INTO emu_sponsors (`profile_id`, `sponsor_id`, `sponsor_points`, `next_unlock_item`) VALUES ({profile_id}, 0, 0, '');");
            SQL.Query($"INSERT INTO emu_sponsors (`profile_id`, `sponsor_id`, `sponsor_points`, `next_unlock_item`) VALUES ({profile_id}, 1, 0, '');");
            SQL.Query($"INSERT INTO emu_sponsors (`profile_id`, `sponsor_id`, `sponsor_points`, `next_unlock_item`) VALUES ({profile_id}, 2, 0, '');");

            //TODO CreateProfile
            //insert SPONSOR
            //сохранять в бд информацию о пк
            //TODO
            //unlocked_item

            foreach (var dItem in Config.DefaultItems)
            {
                //TODO
                var item = Item.DefaultItem(dItem.Name);
                item.SetSlot(dItem.Classes, dItem.Type);
                item.Give(profile_id);
            }
            //UPDATE `emuwarface`.`emu_profiles` SET `experience`='1038495' WHERE  `profile_id`=3;
            //SQL.Query($"UPDATE emu_profiles SET experience={Experience}, current_class={(int)CurrentClass}, height={Height}, fatness={Fatness}, banner_badge={BannerBadge}, banner_mark={BannerMark}, banner_stripe={BannerStripe}, game_money={GameMoney}, cry_money={CryMoney}, crown_money={CrownMoney}, last_seen_date={DateTimeOffset.UtcNow.ToUnixTimeSeconds()} WHERE profile_id={id}");

            //return new Profile() { Id = id };
        }

        public static long GetBanTime(ulong user_id, out string rule)
        {
            rule = string.Empty;

            DataTable db = SQL.QueryRead($"SELECT * FROM emu_bans WHERE user_id={user_id}");

            long unTime = -1;
            foreach(DataRow row in db.Rows)
            {
                var time    = (long)row["unban_time"];
                rule        = (string)row["rule"];

                if (time == 0)
                    return time;

                if (time > DateTimeOffset.UtcNow.ToUnixTimeSeconds() && time > unTime)
                    unTime = time;
            }

            return unTime;
        }

        public static long GetMuteTime(ulong user_id)
        {
            DataTable db = SQL.QueryRead($"SELECT unmute_time FROM emu_mutes WHERE user_id={user_id}");

            long unTime = -1;
            foreach (DataRow row in db.Rows)
            {
                var time = (long)row["unmute_time"];

                if (time == 0)
                    return time;

                if (time > DateTimeOffset.UtcNow.ToUnixTimeSeconds() && time > unTime)
                    unTime = time;
            }

            return unTime;
        }

        public static string GetNickname(ulong profile_id)
        {
            DataTable db = SQL.QueryRead($"SELECT nickname FROM emu_profiles WHERE profile_id={profile_id}");

            return db.Rows.Count == 1 ? db.Rows[0]["nickname"].ToString() : "без_имени_" + profile_id;
        }

        public static string GetNicknameByUserId(ulong userId)
        {
            DataTable db = SQL.QueryRead($"SELECT nickname FROM emu_profiles WHERE user_id={userId}");

            return db.Rows.Count == 1 ? db.Rows[0]["nickname"].ToString() : null;
        }

        public static ulong GetExperience(ulong profile_id)
        {
            DataTable dt = SQL.QueryRead($"SELECT experience FROM emu_profiles WHERE profile_id={profile_id}");

            return dt.Rows.Count == 1 ? (uint)dt.Rows[0]["experience"] : 0;
        }

        public static ulong GetProfileId(string user_id) => GetProfileId(ulong.Parse(user_id));

        public static ulong GetProfileId(ulong user_id)
        {
            DataTable dt = SQL.QueryRead($"SELECT profile_id FROM emu_profiles WHERE user_id={user_id}");

            return dt.Rows.Count == 1 ? ulong.Parse(dt.Rows[0]["profile_id"].ToString()) : 0;
        }

        public static ulong GetProfileIdForNickname(string nickname)
        {
            //MySqlCommand cmd = new MySqlCommand($"SELECT profile_id FROM emu_profiles WHERE nickname={nickname}");
            MySqlCommand cmd = new MySqlCommand($"SELECT profile_id FROM emu_profiles WHERE nickname=@nickname");

            cmd.Parameters.AddWithValue("@nickname", nickname);

            DataTable dt = SQL.QueryRead(cmd);

            return dt.Rows.Count == 1 ? Convert.ToUInt64(dt.Rows[0]["profile_id"]) : 0;
        }

        public static long GetLastSeenDate(ulong profile_id)
        {
            DataTable dt = SQL.QueryRead($"SELECT last_seen_date FROM emu_profiles WHERE profile_id={profile_id}");

            return dt.Rows.Count == 1 ? (long)dt.Rows[0]["last_seen_date"] : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /*public static Jid GetOnlineId(ulong profile_id)
        {
            var res = SQL.QueryRead($"SELECT user_id, status FROM emu_profiles WHERE profile_id={profile_id}");

            if (res.Rows.Count == 0)
                return null;

            return GetOnlineId((PlayerStatus)res.Rows[0]["status"], res.Rows[0]["user_id"].ToString());
        }*/

        /*public static Item GiveItem(Profile profile, string name, ItemType type, int durabilityPoints = 0, long seconds = 0, int quantity = 0)
        {
           *if (profile == null)
                throw new InvalidOperationException();

            return GiveItem(profile.Id, profile.Items, name, type, durabilityPoints, seconds, quantity);
        }*/

        /*public static Item GiveItem(ulong profile_id, string name, ItemType type, int durabilityPoints = 0, long seconds = 0, int quantity = 0)
        {
            var items = Item.GetItems(profile_id);

            return GiveItem(profile_id, items, name, type, durabilityPoints, seconds, quantity);
        }*/

        public Item GiveItem(string name, ItemType type, long seconds = 0, int quantity = 0, int durabilityPoints = 36000)
        {
            lock (Items)
            {
                Item item = Items.FirstOrDefault(item => item.Name == name && item.Type == type);

                if (item != null)
                {
                    switch (type)
                    {
                        case ItemType.Basic:
                        case ItemType.Default:
                            break;
                        case ItemType.Permanent:
                            item.DurabilityPoints += durabilityPoints;
                            item.ExpiredConfirmed = false;
                            break;
                        case ItemType.Consumable:
                            item.Quantity += quantity;
                            item.ExpiredConfirmed = false;
                            break;
                        case ItemType.Expiration:
                            if (item.ExpiredConfirmed)
                            {
                                item.ExpirationTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + seconds;
                                item.ExpiredConfirmed = false;
                            }
                            else
                            {
                                item.ExpirationTimeUtc += seconds;
                            }
                            break;
                    }
                    item.Update();
                }
                else
                {
                    switch (type)
                    {
                        case ItemType.Basic:
                            item = Item.BasicItem(name);
                            break;
                        case ItemType.Permanent:
                            item = Item.PermanentItem(name, durabilityPoints, durabilityPoints);
                            break;
                        case ItemType.Consumable:
                            item = Item.ConsumableItem(name, quantity);
                            break;
                        case ItemType.Expiration:
                            item = Item.ExpirationItem(name, seconds);
                            break;
                    }
                    item.Give(Id);
                    Items.Add(item);
                }
                return item;
            }
        }

        public static ulong GetUserId(ulong profile_id)
        {
            var db = SQL.QueryRead($"SELECT user_id FROM emu_profiles WHERE profile_id={profile_id}");

            if (db.Rows.Count == 0)
                return 0;

            return Convert.ToUInt64(db.Rows[0]["user_id"]);
        }


        public static Profile GetProfileWithUserId(ulong user_id)
        {
            var db = SQL.QueryRead($"SELECT * FROM emu_profiles WHERE user_id={user_id};");

            /*lock (Server.Clients)
            {
                var client = Server.Clients.FirstOrDefault(x => x.UserId == user_id);
                if (client != null && client.Presence.HasFlag(PlayerStatus.Online))
                    return client.Profile;
            }*/

            if (db.Rows.Count == 0)
                return null;

            return ParseDataRow(db.Rows[0]);
        }

        public static Profile GetProfile(ulong profile_id)
        {
            lock (Server.Clients)
            {
                var client = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id);
                if (client != null && client.Profile != null)
                    return client.Profile;
            }

            var db = SQL.QueryRead($"SELECT * FROM emu_profiles WHERE profile_id={profile_id}");

            if (db.Rows.Count == 0)
                return null;

            return ParseDataRow(db.Rows[0]);
        }

        public static Profile GetProfileForNickname(string nickname)
        {
            //TODO проверки на ник
            if (nickname.Length > 16)
                return null;

            lock (Server.Clients)
            {
                var client = Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nickname);
                if (client != null)
                    return client.Profile;
            }

            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM emu_profiles WHERE nickname=@nickname");
            cmd.Parameters.AddWithValue("@nickname", nickname);

            DataTable db = SQL.QueryRead(cmd);

            if (db.Rows.Count == 0)
                return null;

            return ParseDataRow(db.Rows[0]);
        }

        public static string GetOnlineId(ulong profile_id)
        {
            lock (Server.Clients)
            {
                var player = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id);
                return player == null ? "" : player.Jid.ToString();
            }
        }

        public static PlayerStatus GetOnlineStatus(ulong profile_id)
        {
            lock (Server.Clients)
            {
                var player = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id);
                return player == null ? PlayerStatus.Logout : player.Presence;
            }
        }

        public static XmlElement GetInitiatorInfo(ulong profile_id)
        {
            Client client = null;
            lock (Server.Clients)
            {
                client = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id);
            }
            if (client != null)
            {
                return Xml.Element("initiator_info")
                    .Attr("online_id",  client.Jid)
                    .Attr("profile_id", client.ProfileId)
                    .Attr("is_online",  "1")
                    .Attr("name",       client.Profile.Nickname)
                    .Attr("clan_name",  Clan.GetClanName(client.Profile.ClanId))
                    .Attr("experience", client.Profile.Experience)
                    .Attr("badge",      client.Profile.BannerBadge)
                    .Attr("mark",       client.Profile.BannerMark)
                    .Attr("stripe",     client.Profile.BannerStripe);
            }
            else
            {
                Profile profile = GetProfile(profile_id);
                return Xml.Element("initiator_info")
                    .Attr("online_id",  "")
                    .Attr("profile_id", profile.Id)
                    .Attr("is_online",  "0")
                    .Attr("name",       profile.Nickname)
                    .Attr("clan_name",  Clan.GetClanName(client.Profile.ClanId)) //TODO
                    .Attr("experience", profile.Experience)
                    .Attr("badge",      profile.BannerBadge)
                    .Attr("mark",       profile.BannerMark)
                    .Attr("stripe",     profile.BannerStripe);
            }
        }


        private static Profile ParseDataRow(DataRow row)
        {
            var profile = new Profile();
           
            profile.Id = Convert.ToUInt64(row["profile_id"]);
            profile.Nickname = (string)row["nickname"];
            profile.Head = (string)row["head"];
            profile.Height = (int)row["height"];
            profile.Fatness = (int)row["fatness"];
            profile.Experience = (int)row["experience"];
            profile.GameMoney = (int)row["game_money"];
            profile.CrownMoney = (int)row["crown_money"];
            profile.CryMoney = (int)row["cry_money"];
            profile.BannerBadge = (uint)row["banner_badge"];
            profile.BannerMark = (uint)row["banner_mark"];
            profile.BannerStripe = (uint)row["banner_stripe"];
            profile.CurrentClass = (ClassId)(int)row["current_class"];
            profile.IsExperienceFreezed = (byte)row["exp_freezed"] == 1;
            return profile;
            
        }
    }
}
