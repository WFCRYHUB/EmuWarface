using EmuWarface.Core;
using EmuWarface.Game.Enums.Errors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.Shops
{
	public static class RandomBox
	{
		public static ShopErrorCode Open(Profile profile, XmlElement random_box, ref XmlElement purchased_item, ShopOffer offer = null)
		{
			foreach (XmlElement group in random_box.ChildNodes)
			{
				XmlElement win_item = (XmlElement)group.FirstChild;
				int total_weight = 0;

				foreach (XmlElement item in group.ChildNodes)
				{
					total_weight += int.Parse(item.GetAttribute("weight").Replace(".", ""));
				}

				int random = new Random().Next(0, total_weight);

				int current_weight = 0;
				foreach (XmlElement item in group.ChildNodes)
				{
					current_weight += int.Parse(item.GetAttribute("weight").Replace(".", ""));
					if (current_weight >= random)
					{
						win_item = item;
						break;
					}
				}

				//Log.Info($"[RandomBox] win_item={win_item.GetAttribute("name")} random={random} current_weight={current_weight} total_weight={total_weight}");

				ShopErrorCode error = Shop.GiveShopItem(profile, offer, win_item, ref purchased_item);

				if (error != ShopErrorCode.OK)
					return error;
			}

			return ShopErrorCode.OK;
		}
	}
}
