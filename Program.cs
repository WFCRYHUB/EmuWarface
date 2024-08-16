using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Shops;
using EmuWarface.Xmpp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace EmuWarface
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //var s = EmuConfig.API;
            sw.Stop();
            Log.Debug(string.Format("EmuConfig {0},{1}s.", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));
            sw.Restart();

            SQL.Init();
            sw.Stop();
            Log.Debug(string.Format("SQL {0},{1}s.", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));
            sw.Restart();

            QueryBinder.Init();
            sw.Stop();
            Log.Debug(string.Format("QueryBinder {0},{1}s.", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));
            sw.Restart();

            QueryCache.Init();
            sw.Stop();
            Log.Debug(string.Format("QueryCache {0},{1}s.", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));


            GameData.Init();
            sw.Stop();
            Log.Debug(string.Format("GameData {0},{1}s.", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));
            sw.Restart();

            Shop.Init();


            //Server.Init();
            Server.Init();
            //API.Init();
            //Rcon.Init();
            Clan.GenerateClanList();

            //Market.Init();
            //EmuExtensions.UpdateOnline();

            Thread.Sleep(-1);
        }

        public static void Test()
        {
            try
            {
                //var s = double.Parse("23.56", CultureInfo.InvariantCulture);

                //PlayerStat.IncrementPlayerStat(6, "player_kill_streak", 1, mode: PlayMode.PVP);

                //StatsManager.CalculateSessionStats(Xml.Load("Game/telemetry_test.xml"));

                //GameRestrictionSystem.SetDefaultRestrictions(null);

                //BindingList<XmlElement> myList = new BindingList<XmlElement>();

                //myList.AllowEdit = true;
                //myList.RaiseListChangedEvents = true;

                //myList.ListChanged += MyList_ListChanged;

                //myList.Add(Xml.Element("sd"));

                //myList.FirstOrDefault(x => x.Name == "sd")?.Attr("5", "5");

                //var p = Profile.GetProfileWithUserId(3);

                //p.CheckDailyBonus();

                //var type = ParseEnum<RoomType>("46");

                /*var overtime_mode1 = GameModeConfigs.ValidateRestriction("pvp_skilled", "overtime_mode", "3", "ptb", RoomType.PvP_Public);
                var overtime_mode2 = GameModeConfigs.ValidateRestriction("", "overtime_mode", "0", "ptb", RoomType.PvP_Rating);
                var overtime_mode3 = GameModeConfigs.ValidateRestriction("", "max_players", "3", "marseasy", RoomType.PvE);

                var overtime_mode44 = GameModeConfigs.ValidateRestriction("", "overtime_mode", "3", "ffa", RoomType.PvP_Rating);*/

                //QueryBinder.Handler["join_channel"].DynamicInvoke(new Iq(IqType.Error, ""));

                /*foreach (var dItem in Config.DefaultItemsConfig)
                {
                    //TODO
                    var item = new Item(0, dItem.Name, 0, 0, 0, true, false, false, 0, "dm=0;material=default", expirationTime: 0);
                    item.SetSlot(dItem.Type, dItem.Classes);
                    item.Give(223456);
                    Log.Debug(item.Serialize().ToXmlString());
                }*/

                /*var itess = new Item("ar29huy_shop", 1022);
                itess.Give(1);

                var itewms = Item.GetItems(1);

                foreach (var i22 in itewms)
                {
                    Log.Debug(i22.Serialize().ToXmlString());
                }*/


                //var hfd = new Iq(IqType.Get, "", null, new Jid("2@warface/GameClient"), Xml.Parse("<query xmlns='urn:cryonline:k01'><join_channel xmlns='urn:cryonline:k01' version='1.22400.5519.45100' token='$WF_1_1623523854101_30682082bdad47fcdf9373714dad58ee' profile_id='670' user_id='1' region_id='global' hw_id='625666155' cpu_vendor='3' cpu_family='15' head='default_head_19' cpu_model='8' cpu_stepping='2' cpu_speed='3393' cpu_num_cores='6' gpu_vendor_id='4318' gpu_device_id='7298' physical_memory='16314' os_ver='6' os_64='1' language='Russian' build_type='--profile'/></query>"));

                //Profile.CreateProfile(hfd);

                /*var result = SQL.QueryRead($"SELECT * FROM emu_items WHERE id=1441");

                var ggg = new DateTimeOffset((DateTime)result.Rows[0]["buy_time_utc"]);
                var ggg2 = new DateTimeOffset((DateTime)result.Rows[0]["expiration_time_utc"]);

                var s = ggg.ToUnixTimeSeconds();
                var ssw2 = ggg2.ToUnixTimeSeconds();
                var ss = 11212;*/
                //QueryCache.GetServerItems(null, null);

                /*foreach (var item in Config.DefaultItemsConfig)
                {
                    var profileItem = new ProfileItem(item.Name, "dm=0;material=default", isDefault: 1);
                    profileItem.InsertItem(12345);
                    profileItem.SetSlot(ProfileItem.GetSlot(item.Type, item.Classes));
                    //AddItem(item.Name, 1).SetSlot(Item.GetSlot(item.Type, item.Value.Classes));
                    Log.Debug(profileItem.Serialize().OuterXml);
                }*/

                /*var s = ProfileItem.GetAllItems(1);

                var s1 = s[1].IsConsumable();
                var s2 = s[1].IsDurable();
                var s3 = s[1].IsExpirable();
                var s5 = s[1].IsRegular();*/

                //var dataTable = SQL.QueryRead(new MySqlCommand("SELECT * FROM emu_items WHERE id=707"));

                //var s = dataTable.Rows[0]["name"];

                //var s = ProfileItem.GetItemByID(707);
                // var s = ProfileItem.GetAllItems(1);

                //Open server
                //Server.Open(5222);

                //var sfdggd = ProfileItem.GetSlot(ProfileItemSlot.PocketFlashGrenade, new CharacterClass[4] { CharacterClass.Engineer, CharacterClass.Rifleman, CharacterClass.Medic, CharacterClass.Recon });

                /*var ssgfd = PlayerStat.GetPlayerStats(1);

                XmlElement response = Xml.Element("get_player_stats");
                ssgfd.ForEach(x => response.Child(x.Serialize()));

                Log.Debug(response.OuterXml);*/

                /*new Item("kn21", 36000, 36000).Give(3);
                new Item("kn22", 36000, 36000).Give(3);
                new Item("pt21_shop", 36000, 36000).Give(3);
                new Item("pt14_shop", 36000, 36000).Give(3);
                new Item("sr46_shop", 36000, 36000).Give(3);
                new Item("smg51_shop", 36000, 36000).Give(3);
                new Item("ar35_shop", 36000, 36000).Give(3);
                new Item("f_sniper_fbs_02", 36000, 36000).Give(3);
                new Item("f_engineer_fbs_02", 36000, 36000).Give(3);*/

                //new Item("smg46_shop", 36000, 36000).Give(3);

                //Profile.ReSyncProfileItems(new Jid("1@warface/GameClient"), 3);

                //Achievement.GetAchievements(3).ForEach(x => Log.Debug(x.Serialize().OuterXml));
                //<notif id='3177524456' type='2048' confirmation='1' from_jid='masterserver@warface/pve_059_r3' message=''>
                //var notif = new Notification(NotificationType.ItemGiven, true, data: Xml.Parse("<give_item name='ar46_shop' offer_type='Permanent' notify='1'/>"));
                //Notification.AddNotification(3, notif);
                //Notification.SyncNotifications(new Jid("1@warface/GameClient"), notif);
                //Notification.SyncNotifications(new Jid("1@warface/GameClient"), new List<Notification> { notif });

                //Notification.SyncNotifications(new Jid("1@warface/GameClient"));

                //Friend.GetFriends(new Jid("1@warface/GameClient"));
                //Profile.ReSyncProfile(new Jid("3@warface/GameClient"));

                //var notif = new Notification(NotificationType.Message, true, 999999, Xml.Parse("<message data='Пошел нахуй Хаймзон, теперь тут АЛЛОДС'/>"));
                //Notification.AddNotification(3, notif);
                //Notification.SyncNotifications(new Jid("1@warface/GameClient"), new List<Notification> { notif });

                /*var aTimer = new System.Timers.Timer(2000);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;*/

                /*List<string> list = new List<string>();

                foreach(var file in Directory.GetFiles("Game\\Items\\shopitems", "*.xml"))
                {
                    XmlElement shop_item = Xml.Parse(File.ReadAllText(file));

                    string type = shop_item.GetAttribute("type");
                    if (list.Contains(shop_item.GetAttribute("type")))
                        continue;

                    list.Add(type);
                }

                list.ForEach(type => Log.Info($"Type = {type}"));*/

                //var sdd = QueryCache.GetCache("shop_get_offers");

                //Profile.ReSyncProfile(new Jid("3@warface/GameClient"));

                //var online_id = new Jid("3@warface/GameClient");

                //Iq iq = new Iq(IqType.Get, Iq.GenerateId(), online_id, MasterServer.Connection?.Jid);
                //MasterServer.Connection.IqResponse(iq.Result(Xml.Element("delete_item").Attr("item_id", "374")));

                //var notif_del_key = new Notification(NotificationType.ItemDeleted, false, 0, Xml.Element("item_deleted").Attr("profile_item_id", "374"));
                //Notification.SyncNotifications(online_id, new List<Notification> { notif_del_key });

                //Profile.ReSyncProfile(new Jid("1@warface/GameClient"));

                /*Class @class = Class.Rifleman | Class.Medic | Class.Recon;

                var gdsg = (int)@class;
                Console.WriteLine("@Class: {0:G} ({0:D})", @class);

                var gdsgds = @class & Class.Recon;

                if ((@class & Class.Recon) == Class.Recon)
                {

                }

                var item = new Item("dfgdgfsdgs");
                item.SetSlot(@class, BaseSlot.Pistol);

                List<DefaultItemConfig> DefaultItemsConfig = Config.LoadConfig<List<DefaultItemConfig>>("Config/defaultItems.json");


                foreach (var default_item in DefaultItemsConfig)
                {
                    Console.WriteLine("@Class: {0:G} ({0:D})", default_item.Classes);
                }*/

                /*MySqlConnection connection = SQL.GetConnection().GetAwaiter().GetResult();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                DbDataReader reader = new MySqlCommand($"SELECT * FROM emu_items WHERE profile_id=3", connection).ExecuteReader();

                DataTable result = new DataTable();
                result.Load(reader);

                sw.Stop();
                Log.Info("JoinChannel [ITEMS] Time=" + sw.Elapsed.TotalMilliseconds);*/

                //Profile.ReSyncProfile(new Jid("1@warface/GameClient"));

                //XmlElement lock_server = Xml.Element("lock_server");

                //Iq dedic_iq = new Iq(IqType.Get, Iq.GenerateId(), "k01.warface.servers", MasterServer.Connection?.Jid);
                //MasterServer.Connection.IqResponse(dedic_iq.Result(lock_server));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        public static void Test2(object source, ElapsedEventArgs e)
        {
            //MySqlConnection connection = SQL.GetConnection().GetAwaiter().GetResult();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //DbDataReader reader = new MySqlCommand($"SELECT * FROM emu_items WHERE profile_id=3", connection).ExecuteReader();

            //ExecuteScalar
            //object scalar = new MySqlCommand($"SELECT * FROM emu_items WHERE profile_id=3", connection);

            //Console.WriteLine(scalar.ToString());

            /*using (DbDataReader reader = new MySqlCommand($"SELECT * FROM emu_items WHERE profile_id=3 LIMIT 1", connection).ExecuteReader())
            {
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);

            }*/


            sw.Stop();
            Log.Info("JoinChannel [ITEMS] Time=" + sw.Elapsed.TotalMilliseconds);
        }
    }
}
