using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Items;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp.Query;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Commands
{
    public class GiveCommand : ICmd
    {
        public Permission MinPermission => Permission.None;
        public string Usage => "give <nickname> <type> <item_name> [time/count]";
        public string Example => @"
give user1 p ar29_shop
give user1 permanent ar29_shop

give user1 s sniper_fbs_01
give user1 skin sniper_fbs_01

give user1 e ar29_shop 10d
give user1 expiration ar29_shop 10d

give user1 b random_box_10
give user1 box random_box_10

give user1 m game 10000
give user1 m crown 10000
give user1 m cry 10000

give user1 a 2231
give user1 achiev 2231

give user1 all (DISABLE online_use_protect)
";
        public string[] Names => new[] { "give", "g" };

        public string OnCommand(Permission permission, string[] args)
        {
            string result = $"Invalid arguments.\nExample:\n{Example}";

            if (args.Length < 2)
                return result;

            string nickname = args[0];
            string type = args[1];

            if (type != "all" && args.Length < 3)
                return result;
            string itemName = args.Length > 2 ? args[2] : "";

            Profile profile = Profile.GetProfileForNickname(nickname);
            if (profile == null)
            {
                return $"Player with nickname '{nickname}' not found.";
            }

            switch(type)
            {
                case "all":
                    {
                        var items = QueryCache.GetCache("items");

                        foreach (XmlElement item in items.Data.ChildNodes)
                        {
                            string name = item.GetAttribute("name");
                            string max_buy_amount = item.GetAttribute("max_buy_amount");
                            if (max_buy_amount == "1"
                                && !name.Contains("achiev") 
                                && !name.Contains("box")
                                && !name.Contains("bundle")
                                && !name.Contains("unlock"))
                            {
                                var i = profile.GiveItem(item.GetAttribute("name"), ItemType.Basic);
                            }
                        }

                        Client client;
                        lock (Server.Clients)
                        {
                            client = Server.Clients.FirstOrDefault(x => x.Profile?.Nickname == nickname);
                            client.ResyncProfie();
                        }

                        return $"Player with nickname '{profile.Nickname}' was given all shop items.";
                    }
                case "permanent":
                case "p":
                    {
                        return GiveItem(profile, itemName);
                    }
                case "basic": // NO REPAIR
                case "skin":
                case "regular":
                case "r":
                case "s":
                    {
                        return GiveItem(profile, itemName, ItemType.Basic);
                    }
                case "expiration": // FOR TIME
                case "e":
                    {
                        long seconds = Utils.GetTotalSeconds(args[3]);
                        if (seconds == -1)
                        {
                            return $"Invalid time. Example: 1d1h1m (1 day 1 hour 1 minute)";
                        }

                        return GiveItem(profile, itemName, ItemType.Expiration, seconds);
                    }
                case "consumable":
                case "c":
                    {
                        int quantity;
                        if(!int.TryParse(args[3], out quantity) || quantity == 0)
                        {
                            return $"Invalid count ('{args[3]}' not a number).";
                        }

                        return GiveItem(profile, itemName, ItemType.Consumable, quantity);
                    }
                case "box":
                case "b":
                    {
                        if (!ValidateItem(itemName))
                        {
                            return $"Box with name '{itemName}' not found.";
                        }

                        //profile.GiveRandomBoxCards();
                        var notif = profile.GiveRandomBox(itemName);

                        if (notif == null)
                        {
                            return $"Failed to give box. (boxname: {itemName})";
                        }

                        Notification.AddNotification(profile.Id, notif);
                        Notification.SyncNotifications(profile.Id, notif);

                        return $"Player with nickname '{profile.Nickname}' was given box {itemName}.";
                    }
                case "money":
                case "m":
                    {
                        int quantity;
                        if (!int.TryParse(args[3], out quantity) || quantity == 0)
                        {
                            return $"Invalid count ('{args[3]}' not a number).";
                        }

                        switch (itemName)
                        {
                            case "game":
                                profile.GameMoney += quantity;
                                break;
                            case "crown":
                                profile.CrownMoney += quantity;
                                break;
                            case "cry":
                                profile.CryMoney += quantity;
                                break;
                            default:
                                return $"Invalid currency (available 'game', 'crown', 'cry').";
                        }
                        profile.Update();
                        var notif = Notification.GiveMoneyNotification(itemName + "_money", quantity, true);

                        Notification.AddNotification(profile.Id, notif);

                        lock (Server.Clients)
                        {
                            Notification.SyncNotifications(Server.Clients.FirstOrDefault(x => x.ProfileId == profile.Id), notif);
                        }

                        return $"Player with nickname '{nickname}' was given {quantity} {itemName} money.";
                    }
                case "a":
                case "achiev":
                    {
                        uint achiev_id;
                        if (!uint.TryParse(args[3], out achiev_id) || achiev_id == 0)
                        {
                            return $"Invalid achievement id ('{args[3]}' not a number).";
                        }

                        var achiev = Achievement.SetAchiev(profile.Id, achiev_id, int.MaxValue, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        Notification.SyncNotifications(profile.Id, Notification.AchievementNotification(achiev.AchievementId, achiev.Progress, achiev.CompletionTimeUnixTimestamp));

                        return $"Player with nickname '{nickname}' was given achievement with {achiev.AchievementId} id.";
                    }
                default:
                    return "Unknown type. (available: permanent, regular, consumable, box, money, achiev)";
            }
        }

        private string GiveItem(Profile profile, string itemName, ItemType type = ItemType.Permanent, long seconds = 0, int quantity = 0)
        {
            if (!ValidateItem(itemName))
            {
                return $"Item with name '{itemName}' not found.";
            }

            var item = profile.GiveItem(itemName, type, seconds, quantity);

            if (item == null)
            {
                return $"Failed to give item with name '{itemName}'.";
            }

            var notif = Notification.GiveItemNotification(itemName, type.ToString(), true, seconds, quantity);

            Notification.AddNotification(profile.Id, notif);

            lock (Server.Clients)
            {
                Notification.SyncNotifications(Server.Clients.FirstOrDefault(x => x.ProfileId == profile.Id), notif);
            }

            return $"Player with nickname '{profile.Nickname}' was given item {itemName}.";
        }

        private bool ValidateItem(string item_name)
        {
            var items = QueryCache.GetCache("items");

            foreach (XmlElement item in items.Data.ChildNodes)
            {
                if (item.GetAttribute("name") == item_name)
                    return true;
            }

            return false;
        }
    }
}
