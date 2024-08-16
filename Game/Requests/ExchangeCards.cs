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
    public static class ExchangeCards
    {
        //<exchange_cards currency="none" level="2" />
        [Query(IqType.Get, "exchange_cards")]
        public static void ExchangeCardsSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            var currency    = q.GetAttribute("currency");
            var level       = int.Parse(q.GetAttribute("level"));

            var leftover_card = client.Profile.Items.FirstOrDefault(x => x.Name == "leftover_card");

            if (leftover_card == null || leftover_card.Quantity == 0)
                throw new QueryException(1);

            var rate    = 0;
            var price   = 0;

            switch (currency)
            {
                case "none":
                    {
                        //15:1
                        rate = 15;
                    }
                    break;
                case "game_money":
                    {
                        //10:1  -7500
                        rate    = 10;
                        price   = 7500;
                    }
                    break;
                case "crown_money":
                    {
                        //5:1  -75
                        rate    = 5;
                        price   = 75;
                    }
                    break;
                case "cry_money":
                    {
                        //2:1  -4
                        rate    = 2;
                        price   = 4;
                    }
                    break;
                default:
                    throw new QueryException(1);
            }

            rate    *= level;
            price   *= level;

            if (rate > leftover_card.Quantity)
                throw new QueryException(1);

            switch (currency)
            {
                case "game_money":
                    {
                        if(price > client.Profile.GameMoney)
                        {
                            throw new QueryException(1);
                        }
                        else
                        {
                            client.Profile.GameMoney -= price;
                        }
                    }
                    break;
                case "crown_money":
                    {
                        if (price > client.Profile.CrownMoney)
                        {
                            throw new QueryException(1);
                        }
                        else
                        {
                            client.Profile.CrownMoney -= price;
                        }
                    }
                    break;
                case "cry_money":
                    {
                        if (price > client.Profile.CryMoney)
                        {
                            throw new QueryException(1);
                        }
                        else
                        {
                            client.Profile.CryMoney -= price;
                        }
                    }
                    break;
            }

            leftover_card.Quantity -= rate;
            leftover_card.Update();

            client.Profile.GiveItem("free_card", ItemType.Consumable, quantity: level);
            client.Profile.Update();

            q.Attr("leftover_cards_left", leftover_card.Quantity);

            //iq.SetQuery(exchange_cards);
            client.QueryResult(iq);
        }
    }
}
