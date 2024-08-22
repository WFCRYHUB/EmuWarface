using EmuWarface.Core;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Game.Shops;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class ShopBuyOffer
    {
        [Query(IqType.Get, "shop_buy_offer", "shop_buy_multiple_offer", "extend_item")]
        public static void ShopBuyMultipleOffer(Client client, Iq iq)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //<shop_buy_offer supplier_id="1" hash="-2075368079" offer_id="304541" />
            //<shop_buy_multiple_offer supplier_id="1" hash="-2075368079"><offer id="303842" /><offer id="303842" /></shop_buy_multiple_offer>

            XmlElement shop_buy_offer = Xml.Element(iq.Query.LocalName);
            XmlElement purchased_item = Xml.Element("purchased_item");

            if (client.Profile == null)
                throw new InvalidOperationException();

            ShopErrorCode error = ShopErrorCode.OK;

            if (iq.Query.GetAttribute("hash") != Shop.Hash)
            {
                error = ShopErrorCode.HashMismatch;
            }
            else
            {

                List<int> buy_offer_list = new List<int>();
                if (shop_buy_offer.LocalName == "shop_buy_offer")
                {
                    buy_offer_list.Add(int.Parse(iq.Query.GetAttribute("offer_id")));
                }
                else
                {
                    foreach (XmlElement offer_node in iq.Query.ChildNodes)
                    {
                        buy_offer_list.Add(int.Parse(offer_node.GetAttribute("id")));
                    }
                }

                foreach (int offer_id in buy_offer_list)
                {
                    var offer = Shop.Offers.FirstOrDefault(x => x.Id == offer_id);
                    if (offer == null)
                    {
                        error = ShopErrorCode.OutOfStore;
                        break;
                    }

                    try
                    {
                        error = Shop.BuyShopOffer(client, offer, ref purchased_item);

                        if (error != ShopErrorCode.OK)
                            break;
                    }
                    catch (Exception e)
                    {
                        Log.Error("[Shop] Exception " + e.ToString());
                        throw new QueryException(1);
                    }
                }
            }

            if (error == ShopErrorCode.OK)
            {
                shop_buy_offer.Child(purchased_item);
                shop_buy_offer.Child(Xml.Element("money")
                    .Attr("game_money", client.Profile.GameMoney)
                    .Attr("cry_money", client.Profile.CryMoney)
                    .Attr("crown_money", client.Profile.CrownMoney));

                //client.Profile.Update();
            }
            //TODO else reload profile

            client.Profile.GetRank();

            iq.SetQuery(shop_buy_offer.Attr("error_status", (int)error));
            client.QueryResult(iq);
        }
    }
}
