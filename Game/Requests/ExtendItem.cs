using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.Shops;
using EmuWarface.Game.Enums.Errors;
using EmuWarface.Xmpp;
using System;
using System.Xml;
using System.Linq;

namespace EmuWarface.Game.Requests
{
    public static class PlayerStatusSExtendItemerializer
    {
        //<extend_item item_id='398' supplier_id='1' offer_id='300761' hash='808341463'/>

        [Query(IqType.Get, "extend_item")]
        public static void ExtendItem(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            var item_id = iq.Query.GetAttribute("item_id");
            var offer_id = int.Parse(iq.Query.GetAttribute("offer_id"));

            XmlElement extend_item = Xml.Element(iq.Query.LocalName);

            if (iq.Query.GetAttribute("hash") != Shop.Hash)
            {
                iq.SetQuery(extend_item.Attr("error_status", (int)ShopErrorCode.HashMismatch));
                client.QueryResult(iq);
            }

            var offer = Shop.Offers.FirstOrDefault(x => x.Id == offer_id);
            if (offer == null)
            {
                iq.SetQuery(extend_item.Attr("error_status", (int)ShopErrorCode.OutOfStore));
                client.QueryResult(iq);
            }

            XmlElement purchased_item = Xml.Element("purchased_item");
            try
            {
                ShopErrorCode error = Shop.BuyShopOffer(client, offer, ref purchased_item);

                //TODO temp
                if (error == ShopErrorCode.InvalidRequest)
                    throw new QueryException(1);

                if (error != ShopErrorCode.OK)
                {
                    iq.SetQuery(extend_item.Attr("error_status", (int)error));
                    client.QueryResult(iq);
                }
            }
            catch (Exception e)
            {
                Log.Error("[Shop] Exception " + e.ToString());
                throw new QueryException(1);
            }

            /*
<purchased_item>
<profile_item name="smokegrenade02_c" profile_item_id="399" offerId="300657" added_expiration="0" added_quantity="10" error_status="0">
<item id="399" name="smokegrenade02_c" attached_to="0" config="dm=0;material=" slot="1073741824" equipped="0" default="0" permanent="0" expired_confirmed="1" buy_time_utc="0" quantity="110" />
</profile_item>
</purchased_item>
             */
            var item = (XmlElement)purchased_item.FirstChild?.FirstChild;
            if (item != null)
            {
                extend_item.Attr("error_status", (int)ShopErrorCode.OK)
                    .Attr("durability", item.HasAttribute("durability_points") ? item.GetAttribute("durability_points") : "0")
                    .Attr("expiration_time_utc", item.HasAttribute("expiration_time_utc") ? item.GetAttribute("expiration_time_utc") : "0")
                    .Attr("total_durability", item.HasAttribute("durability_points") ? item.GetAttribute("durability_points") : "0")
                    .Attr("seconds_left", item.HasAttribute("expiration_time_utc") ? (int.Parse(item.GetAttribute("expiration_time_utc")) - int.Parse(item.GetAttribute("buy_time_utc"))).ToString() : "0")
                    .Attr("cry_money", client.Profile.CryMoney)
                    .Attr("game_money", client.Profile.GameMoney)
                    .Attr("crown_money", client.Profile.CrownMoney);
            }
            else
            {
                iq.SetQuery(extend_item.Attr("error_status", (int)ShopErrorCode.Not_Changed));
                client.QueryResult(iq);
            }
            /*
             xElement3.Add(new XAttribute("durability", Buyed.DurabilityPoints));
				xElement3.Add(new XAttribute("total_durability", Buyed.TotalDurabilityPoints));
				xElement3.Add(new XAttribute("expiration_time_utc", Buyed.ExpirationTime));
				xElement3.Add(new XAttribute("seconds_left", Buyed.SecondsLeft));
				xElement3.Add(new XAttribute("cry_money", User.Player.CryMoney));
				xElement3.Add(new XAttribute("game_money", User.Player.GameMoney));
				xElement3.Add(new XAttribute("crown_money", User.Player.CrownMoney));
             */

            iq.SetQuery(extend_item);
            client.QueryResult(iq);
        }
    }
}
