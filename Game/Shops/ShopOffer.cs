using EmuWarface.Game.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EmuWarface.Game.Shops
{
    public class ShopOffer
    {
        /*<offer id='8440' expirationTime='1 day' durabilityPoints='0' repair_cost='0' quantity='0' name='sniper_helmet_crown_02' item_category_override='' offer_status='normal' 
         * supplier_id='1' discount='0' rank='0' game_price='0' cry_price='0' crown_price='250' game_price_origin='0' cry_price_origin='0' crown_price_origin='250' key_item_name=''/>*/

        int _repairCost;
        WinItem[] _winItems;

        public int Id { get; }
        public string ExpirationTime { get; }
        public int DurabilityPoints { get; }

        public int RepairCost
        {
            get
            {
                if (HasWinItems)
                    throw new InvalidOperationException();
                return _repairCost;
            }
            set => _repairCost = value;
        }

        public int Quantity { get; }
        public string Name { get; }
        public string ItemCategoryOverride { get; }
        public string OfferStatus { get; }
        public int SupplierId { get; }
        public int Discount { get; }
        public int SortingIndex { get; }
        public int Rank { get; }
        public int GamePrice { get; }
        public int CryPrice { get; }
        public int CrownPrice { get; }
        public int GamePriceOrigin { get; }
        public int CryPriceOrigin { get; }
        public int CrownPriceOrigin { get; }
        public string KeyItemName { get; }

        public WinItem[] WinItems
        {
            get
            {
                if (!HasWinItems)
                    throw new InvalidOperationException();
                return _winItems;
            }
            set => _winItems = value;
        }

        public bool HasWinItems => _repairCost == -1;

        public Currency Currency
        {
            get
            {
                if (GamePrice != 0)
                    return Currency.Game;
                if (CryPrice != 0)
                    return Currency.Cry;
                if (CrownPrice != 0)
                    return Currency.Crowns;
                throw new InvalidOperationException();
            }
        }

        protected ShopOffer(int id, string expirationTime, int durabilityPoints, int repairCost, int quantity, string name, string itemCategoryOverride, string offerStatus, int supplierId, int discount, int sortingIndex, int rank, int gamePrice, int cryPrice, int crownPrice, int gamePriceOrigin, int cryPriceOrigin, int crownPriceOrigin, string keyItemName, WinItem[] winItems)
        {
            Id = id;
            ExpirationTime = expirationTime;
            DurabilityPoints = durabilityPoints;
            RepairCost = repairCost;
            Quantity = quantity;
            Name = name;
            ItemCategoryOverride = itemCategoryOverride;
            OfferStatus = offerStatus;
            SupplierId = supplierId;
            Discount = discount;
            SortingIndex = sortingIndex;
            Rank = rank;
            GamePrice = gamePrice;
            CryPrice = cryPrice;
            CrownPrice = crownPrice;
            GamePriceOrigin = gamePriceOrigin;
            CryPriceOrigin = cryPriceOrigin;
            CrownPriceOrigin = crownPriceOrigin;
            KeyItemName = keyItemName;
            WinItems = winItems;
        }

        public static long ParseSeconds(string data)
        {
            long seconds = 0;
            if (data.Contains("month") || data.Contains("m"))
            {
                seconds = long.Parse(data.Replace("month", "").Replace("m", "")) * 60 * 60 * 24 * 30;
            }
            else if (data.Contains("day") || data.Contains("d"))
            {
                seconds = long.Parse(data.Replace("day", "").Replace("d", "")) * 60 * 60 * 24;
            }
            else if (data.Contains("hour") || data.Contains("h"))
            {
                seconds = long.Parse(data.Replace("hour", "").Replace("h", "")) * 60 * 60;
            }

            return seconds;
        }

        public static ShopOffer ParseNode(XmlElement offerNode)
        {
            int id =                int.Parse(offerNode.GetAttribute("id"));
            int durabilityPoints =  int.Parse(offerNode.GetAttribute("durabilityPoints"));
            string expirationTime = offerNode.GetAttribute("expirationTime");

            string repairCostRaw = offerNode.Attributes["repair_cost"].Value;
            int repairCost = -1;
            List<WinItem> winItems = null;
            if (string.IsNullOrEmpty(repairCostRaw))
            {
                repairCost = 0;
            }
            else if (int.TryParse(repairCostRaw, out repairCost))
            {
                //repairCost = Convert.ToInt32(repairCostRaw);
            }
            else if (repairCostRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Length > 0)
            {
                var repairSplit = repairCostRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                winItems = new List<WinItem>();
                foreach (string repairEntry in repairSplit)
                {
                    //sr31_shop,5400,36000
                    var repairEntrySplit = repairEntry.Split(',');
                    string itemName = repairEntrySplit[0];
                    int itemRepairCost = int.Parse(repairEntrySplit[1]);
                    int itemDurability = int.Parse(repairEntrySplit[2]);
                    winItems.Add(new WinItem(itemName, itemRepairCost, itemDurability));
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            int quantity = int.Parse(offerNode.GetAttribute("quantity"));
            string name = offerNode.GetAttribute("name");
            string itemCategoryOverride = offerNode.GetAttribute("item_category_override");
            string offerStatus = offerNode.GetAttribute("offer_status");
            int supplierId = int.Parse(offerNode.GetAttribute("supplier_id"));
            int discount = int.Parse(offerNode.GetAttribute("discount"));
            int rank = int.Parse(offerNode.GetAttribute("rank"));
            int sortingIndex = int.Parse(offerNode.GetAttribute("sorting_index"));
            int gamePrice = int.Parse(offerNode.GetAttribute("game_price"));
            int cryPrice = int.Parse(offerNode.GetAttribute("cry_price"));
            int crownPrice = int.Parse(offerNode.GetAttribute("crown_price"));
            int gamePriceOrigin = int.Parse(offerNode.GetAttribute("game_price_origin"));
            int cryPriceOrigin = int.Parse(offerNode.GetAttribute("cry_price_origin"));
            int crownPriceOrigin = int.Parse(offerNode.GetAttribute("crown_price_origin"));
            string keyItemName = offerNode.GetAttribute("key_item_name");


            return new ShopOffer(id, expirationTime, durabilityPoints, repairCost, quantity, name, itemCategoryOverride, offerStatus,
                supplierId, discount, sortingIndex, rank, gamePrice, cryPrice, crownPrice, gamePriceOrigin, cryPriceOrigin, crownPriceOrigin, keyItemName, winItems?.ToArray());
        }
    }
}
