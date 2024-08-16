using EmuWarface.Core;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Items;
using EmuWarface.Game.Notifications;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.Shops
{
	public static class Shop
	{
		public static List<XmlElement> ShopItems = new List<XmlElement>();
		public static List<ShopOffer> Offers = new List<ShopOffer>();

		public static string Hash { get; private set; }

		public static void Init()
		{
			UpdateShopOffers(QueryCache.GetCache("shop_get_offers")?.Data);

			QueryCache.OnUpdate += UpdateShopOffers;

			foreach (var file in Directory.GetFiles(GameDataConfig.SHOPITEMS_CONFIG_FOLDER, "*.xml", SearchOption.AllDirectories))
			{
				try
				{
					ShopItems.Add(Xml.Load(file));
				}
				catch (Exception e)
				{
					Log.Error("[Shop] Failed to parse ShopOffer: {0}", file);
					throw e;
				}
			}

			Log.Info("[Shop] Loaded {0} ShopItems", ShopItems.Count);
			Log.Info("[Shop] Loaded {0} Offers",	Offers.Count);
		}

		public static void UpdateShopOffers(object sender, string name)
		{
			if (name == "shop_get_offers")
			{
				UpdateShopOffers((XmlElement)sender);
			}
		}

		public static void UpdateShopOffers(XmlElement data)
		{
			Hash = data.GetAttribute("hash");
            try
            {
				foreach (XmlElement offer in data.ChildNodes)
				{
					Offers.Add(ShopOffer.ParseNode(offer));
				}
			}
			catch(Exception e)
            {
				Log.Error(e.ToString());
            }
		}

		public static ShopErrorCode BuyShopOffer(Client client, ShopOffer offer, ref XmlElement purchased_item)
		{
			if (offer.GamePrice		> client.Profile.GameMoney ||
				offer.CryPrice		> client.Profile.CryMoney ||
				offer.CrownPrice	> client.Profile.CrownMoney)
			{
				return ShopErrorCode.NotEnoughMoney;
			}

			Item item = null;

			XmlElement shopItem = ShopItems.FirstOrDefault(x => x.GetAttribute("name") == offer.Name);
			if (shopItem != null)
			{
				string type = shopItem.GetAttribute("type");

				//Log.Info($"[Shop] [shop_item] type = {type}");

				switch (type)
				{
					case "mission_access":
					case "coin":
					case "clan":
						item = client.Profile.GiveItem(offer.Name, ItemType.Consumable, quantity: offer.Quantity);
						client.Profile.Room?.ShopSyncConsumables(client.ProfileId, item.Serialize());
						break;
					case "booster":
						long seconds = ShopOffer.ParseSeconds(offer.ExpirationTime);
						item = client.Profile.GiveItem(offer.Name, ItemType.Expiration, seconds: seconds);
						break;
					case "game_money":
						client.Profile.GameMoney += offer.Quantity;
						break;
					case "meta_game":
						var achiev_id	= uint.Parse(shopItem["metagame_stats"]["on_activate"].GetAttribute("unlock_achievement"));
						var achiev		= Achievement.SetAchiev(client.ProfileId, achiev_id, 1, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

						item = client.Profile.GiveItem(offer.Name, ItemType.Basic);
						Notification.SyncNotifications(client, Notification.AchievementNotification(achiev_id, achiev.Progress, achiev.CompletionTimeUnixTimestamp));
						break;
					case "bundle":
						{
							foreach (XmlElement bundle_item in shopItem["bundle"].ChildNodes)
							{
								ShopErrorCode error = GiveShopItem(client.Profile, offer, bundle_item, ref purchased_item);

								if (error != ShopErrorCode.OK)
									return error;
							}
							break;
						}
					case "random_box":
                        {
							RandomBox.Open(client.Profile, shopItem["random_box"], ref purchased_item, offer);
						}
						break;
					default:
						return ShopErrorCode.KeyTimeOut;
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(offer.KeyItemName))
				{
					var key_item = client.Profile.Items.FirstOrDefault(item => item.Name == offer.KeyItemName);
					if (key_item == null && !key_item.ExpiredConfirmed)
					{
						return ShopErrorCode.KeyTimeOut;
					}
					else
					{
						key_item.Delete();

						client.Profile.Items.Remove(key_item);
						Notification.SyncNotifications(client, Notification.DeleteItemNotification(key_item.Id));
					}
				}

				if (offer.ExpirationTime != "0")
				{
					long seconds = ShopOffer.ParseSeconds(offer.ExpirationTime);
					item = client.Profile.GiveItem(offer.Name, ItemType.Expiration, seconds: seconds);
				}
				else if (offer.DurabilityPoints != 0)
				{
					item = client.Profile.GiveItem(offer.Name, ItemType.Permanent, durabilityPoints: offer.DurabilityPoints);
				}
				else if (offer.Quantity != 0)
				{
					item = client.Profile.GiveItem(offer.Name, ItemType.Consumable, quantity: offer.Quantity);
				}
				else
				{
					item = client.Profile.GiveItem(offer.Name, ItemType.Basic);
				}

				if (item == null)
					return ShopErrorCode.LimitReached;
			}

			if (item != null)
			{
				XmlElement result = Xml.Element("profile_item")
						.Attr("name",				item.Name)
						.Attr("profile_item_id",	item.Id)
						.Attr("offerId",			offer.Id)
						.Attr("added_expiration",	offer.ExpirationTime)
						.Attr("added_quantity",		offer.Quantity)
						.Attr("error_status", (int)ShopErrorCode.OK);
				result.Child(item.Serialize());

				purchased_item.Child(result);
			}

			client.Profile.GameMoney -= (int)offer.GamePrice;
			client.Profile.CryMoney -= (int)offer.CryPrice;
			client.Profile.CrownMoney -= (int)offer.CrownPrice;

			client.Profile.Update();
			
			return ShopErrorCode.OK;
		}

		public static ShopErrorCode GiveShopItem(Profile profile, ShopOffer offer, XmlElement item, ref XmlElement purchased_item)
		{
			//<item name="kn10" expiration="7d" weight="8"/>				
			//<item name="smokegrenade03_c" amount="7" weight="10"/>
			//<item name="pt33_gold01_shop" weight="1" top_prize_token="box_token_cry_money_95" win_limit="1000"/>

			var name				= item.GetAttribute("name");
			var expiration			= item.HasAttribute("expiration") ? ShopOffer.ParseSeconds(item.GetAttribute("expiration")) : 0;
			var amount				= item.HasAttribute("amount") ? int.Parse(item.GetAttribute("amount")) : 0;
			var win_limit			= item.HasAttribute("win_limit") ? int.Parse(item.GetAttribute("win_limit")) : 0;
			var top_prize_token		= item.HasAttribute("top_prize_token") ? item.GetAttribute("top_prize_token") : string.Empty;

			XmlElement shopItem = ShopItems.FirstOrDefault(x => x.GetAttribute("name") == item.GetAttribute("name"));
			if (shopItem != null)
			{
				//Log.Info($"[Shop] [BuyShopItem] name = {name}");

				string type = shopItem.GetAttribute("type");

				switch (type)
				{
					case "booster":
						{
							Item _item = profile.GiveItem(name, ItemType.Expiration, seconds: expiration);

							XmlElement result = Xml.Element("profile_item")
								.Attr("name", _item.Name)
								.Attr("profile_item_id", _item.Id)
								.Attr("offerId", offer == null ? 0 : offer.Id)
								.Attr("added_expiration", item.GetAttribute("expiration"))
								.Attr("added_quantity", item.GetAttribute("amount"))
								.Attr("error_status", (int)ShopErrorCode.OK);
							result.Child(_item.Serialize());

							purchased_item.Child(result);
							break;
						}
					case "random_box":
						{
							return RandomBox.Open(profile, shopItem["random_box"], ref purchased_item, offer);
						}
					case "bundle":
						{
							foreach (XmlElement bundle_item in shopItem["bundle"].ChildNodes)
							{
								ShopErrorCode error = GiveShopItem(profile, offer, bundle_item, ref purchased_item);

								if (error != ShopErrorCode.OK)
									return error;
							}
							break;
						}
					case "top_prize_token":
						{
							return ShopErrorCode.KeyTimeOut;
						}
					case "coin":
						{
							Item _item = profile.GiveItem(name, ItemType.Consumable, quantity: amount);

							XmlElement result = Xml.Element("profile_item")
								.Attr("name", _item.Name)
								.Attr("profile_item_id", _item.Id)
								.Attr("offerId", offer == null ? 0 : offer.Id)
								.Attr("added_expiration", item.GetAttribute("expiration")) //TODO test GiveShopItem expiration
								.Attr("added_quantity", item.GetAttribute("amount"))
								.Attr("error_status", (int)ShopErrorCode.OK);
							result.Child(_item.Serialize());

							purchased_item.Child(result);
							//TODO shop_sync_consumables
							break;
						}
					case "contract":
						{
							return ShopErrorCode.KeyTimeOut;
						}
					case "crown_money":
						{
							profile.CrownMoney += (int)amount;

							XmlElement result = Xml.Element("crown_money")
								.Attr("name", name)
								.Attr("added", amount)
								.Attr("total", profile.CrownMoney)
								.Attr("offerId", offer == null ? 0 : offer.Id);

							purchased_item.Child(result);
							break;
						}
					case "cry_money":
						{
							profile.CryMoney += (int)amount;

							XmlElement result = Xml.Element("cry_money")
								.Attr("name", name)
								.Attr("added", amount)
								.Attr("total", profile.CryMoney)
								.Attr("offerId", offer == null ? 0 : offer.Id);

							purchased_item.Child(result);
							break;
						}
					case "exp":
						{
							if (profile.IsExperienceFreezed)
								break;

							profile.Experience += (int)amount;

							XmlElement result = Xml.Element("exp")
								.Attr("name", name)
								.Attr("added", amount)
								.Attr("total", profile.Experience)
								.Attr("offerId", offer == null ? 0 : offer.Id);

							purchased_item.Child(result);
							break;
						}
					case "game_money":
						{
							profile.GameMoney += (int)amount;

							XmlElement result = Xml.Element("game_money")
								.Attr("name", name)
								.Attr("added", amount)
								.Attr("total", profile.GameMoney)
								.Attr("offerId", offer == null ? 0 : offer.Id);

							purchased_item.Child(result);
							break;
						}
					case "key":
						{
							return ShopErrorCode.KeyTimeOut;
						}
					//case "mission_access":
					//	{
					//		return ShopErrorCode.KeyTimeOut;
					//	}
					case "meta_game":
						{
							return ShopErrorCode.KeyTimeOut;
						}
					default:
						return ShopErrorCode.KeyTimeOut;
				}
			}
			else
			{
				Item _item = null;

				//TODO Если скины в коробке то че делать пиздец нахуй
				if (expiration != 0)
				{
					_item = profile.GiveItem(name, ItemType.Expiration, seconds: expiration);
				}
				else if (amount != 0)
				{
					_item = profile.GiveItem(name, ItemType.Consumable, quantity: amount);
				}
				else if(name.Contains("fbs"))
				{
					_item = profile.GiveItem(name, ItemType.Basic);
				}
                else
                {
					_item = profile.GiveItem(name, ItemType.Permanent, durabilityPoints: 36000);
				}

				if (_item == null)
					return ShopErrorCode.LimitReached;

				XmlElement result = Xml.Element("profile_item")
						.Attr("name", _item.Name)
						.Attr("profile_item_id", _item.Id)
						.Attr("offerId", offer == null ? 0 : offer.Id)
						.Attr("added_expiration", item.GetAttribute("expiration")) //TODO test GiveShopItem expiration
						.Attr("added_quantity", item.GetAttribute("amount"))
						.Attr("error_status", (int)ShopErrorCode.OK);
				result.Child(_item.Serialize());

				purchased_item.Child(result);
			}

			return ShopErrorCode.OK;
		}
	}
}
