using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Items;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class FinishCardProgression
    {
        //<finish_card_progression card="engineer_helmet_heist_01_card" />
        [Query(IqType.Get, "finish_card_progression")]
        public static void FinishCardProgressionSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            var card                        = q.GetAttribute("card");
            var card_progressions_config    = QueryCache.GetCache("get_configs").Data["card_progressions_config"];

            var card_item = client.Profile.Items.FirstOrDefault(x => x.Name == card);

            if (card_item == null)
                card_item = client.Profile.GiveItem(card, ItemType.Consumable);

            Item item = null;
            Item free_card_item = client.Profile.Items.FirstOrDefault(x => x.Name == "free_card");
            int free_cards = free_card_item != null ? free_card_item.Quantity : 0;

            //<progression name="ar38_card" cards_required="1000" item="ar38_shop" game_money="30000"/>
            foreach (XmlElement progression in card_progressions_config.ChildNodes)
            {
                if (progression.GetAttribute("name") != card)
                    continue;

                var cards_required      = int.Parse(progression.GetAttribute("cards_required"));
                var game_money_price    = int.Parse(progression.GetAttribute("game_money"));

                if(game_money_price > client.Profile.GameMoney)
                    throw new QueryException(1);

                // 100 > 50  &&  50 - 100 > 1000
                if (cards_required > card_item.Quantity && cards_required - card_item.Quantity > free_cards) 
                    throw new QueryException(1);

                client.Profile.GameMoney -= game_money_price;
                client.Profile.Update();

                var rest = Math.Clamp(cards_required - card_item.Quantity, 0, cards_required);

                card_item.Quantity -= cards_required - rest;
                card_item.Update();

                if(rest > 0)
                {
                    free_card_item.Quantity -= rest;
                    free_card_item.Update();
                }

                q.Attr("specific_cards_spent",  cards_required - rest);
                q.Attr("free_cards_spent",      rest);

                //specific_cards_spent="105" free_cards_spent="-5"

                item = client.Profile.GiveItem(progression.GetAttribute("item"), ItemType.Permanent);
            }

            if (item == null)
                throw new QueryException(1);

            //XmlElement finish_card_progression = Xml.Element("finish_card_progression")
            //    .Attr("card", card)
                //.Attr("specific_cards_spent", 1)
                //.Attr("free_cards_spent", 1);
                //.Child(item.Serialize());

            //client.QueryGet(client.Profile.ResyncProfie());

            //iq.SetQuery(finish_card_progression);
            client.QueryResult(iq);
        }
    }
}
