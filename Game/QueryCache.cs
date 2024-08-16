using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml;

namespace EmuWarface.Game
{
    //TODO весь класс переделать
    public static class QueryCache
    {
        private static Dictionary<string, Cache> caches = new Dictionary<string, Cache>();
        private static string[] cache_names =
        {
            "items",
            "get_configs",
            "shop_get_offers",
            "quickplay_maplist",
            "get_battle_pass_season"
        };

        public static event EventHandler<string> OnUpdate;

        static QueryCache()
        {
            foreach(string name in cache_names)
            {
                caches[name] = Load(name);
            }
        }

        public static void Init()
        {
            var timer = new Timer(5000)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += ValidateFiles;
        }

        static Cache Load(string name, string data = null, string hash = null)
        {
            if (string.IsNullOrEmpty(data))
            {
                data = File.ReadAllText("Game/cache/" + name + ".xml");
            }
            if (string.IsNullOrEmpty(hash))
            {
                hash = EmuExtensions.GetHash(data).ToString();
            }

            //TODO try
            XmlElement element = Xml.Parse(data)
                .Attr("hash", hash);

            var splitted = new List<XmlElement>();
            Split(element, ref splitted, hash);

            Log.Info($"[QueryCache] Loaded '{name}'");
            Log.Info($"[QueryCache] Hash: {hash}");

            return new Cache(element, splitted, hash);
        }

        static void ValidateFiles(object source, ElapsedEventArgs e)
        {
            foreach (var name in cache_names)
            {
                string data = File.ReadAllText("Game/cache/" + name + ".xml");
                string hash = EmuExtensions.GetHash(data).ToString();

                if (caches.ContainsKey(name) && caches[name].Hash != hash)
                {
                    Log.Info($"[QueryCache] '{name}' was changed");
                    caches[name] = Load(name, data, hash);

                    OnUpdate?.Invoke(caches[name].Data, name);
                }
            }
        }

        static void Split(XmlElement el, ref List<XmlElement> list, string hash)
        {
            int blockSize = 250;
            XmlNodeList items = el.ChildNodes;

            int itemsLength = items.Count;
            int blocksLength = itemsLength / blockSize;
            int from = 0;

            for (int i = 0; i <= blocksLength; i++)
            {
                XmlElement result = Xml.Element("items")
                    .Attr("from", from)
                    .Attr("to", from + blockSize)
                    .Attr("code", 2)
                    .Attr("hash", hash);

                if (blocksLength == i)
                {
                    blockSize = itemsLength - from;

                    if (blockSize == 0)
                    {
                        list.Last().Attr("code", 3);
                        break;
                    }

                    result.Attr("code", 3);
                }

                for (int a = 0; a < blockSize; a++)
                {
                    result.Child(items[from]);
                    from++;
                }

                list.Add(result);
            }
        }

        /*[Query("shop_get_offers", IqType.Get)]
        public static void GetOfferList(Client client, Iq iq) => GetCachedQuery(client, iq);
        [Query("items", IqType.Get)]
        public static void GetItems(Client client, Iq iq) => GetCachedQuery(client, iq);
        [Query("get_configs", IqType.Get)]
        public static void GetConfigs(Client client, Iq iq) => GetCachedQuery(client, iq);
        [Query("quickplay_maplist", IqType.Get)]
        public static void GetQuickplayMapList(Client client, Iq iq) => GetCachedQuery(client, iq);
        [Query("get_battle_pass_season", IqType.Get)]
        public static void GetBattlePassSeason(Client client, Iq iq) => GetCachedQuery(client, iq);*/

        //TODO
        [Query(IqType.Get, "get_battle_pass_season", "quickplay_maplist", "get_configs", "items", "shop_get_offers")]
        public static void GetCachedQuery(Client client, Iq iq)
        {
            Cache cache = caches[iq.Query.LocalName];
            var splitted = cache.Splitted;

            int.TryParse(iq.Query?.GetAttribute("from"), out int from);
            int index = from / 250;
            string hash = cache.Hash;

            XmlElement res = Xml.Element(iq.Query.LocalName);

            if (iq.Query?.GetAttribute("cached") == splitted[0].GetAttribute("hash"))
            {
                res.Attr("code", "1")
                    .Attr("from", "0")
                    .Attr("to", "0")
                    .Attr("hash", hash);
            }
            else if (splitted != null && splitted[index] != null)
            {
                res = splitted[index];
            }

            iq.To = Server.Channels.First().Jid;
            iq.SetQuery(res);

            client.QueryResult(iq);
        }

        public static Cache GetCache(string name)
        {
            if (caches.ContainsKey(name))
                return caches[name];
            else
                return null;
        }

        public class Cache
        {
            public XmlElement Data { get; private set; }
            public List<XmlElement> Splitted { get; private set; }
            public string Hash { get; private set; }

            public Cache(XmlElement data, List<XmlElement> splitted, string hash)
            {
                Data = data;
                Splitted = splitted;
                Hash = hash;
            }
        }
    }
}
