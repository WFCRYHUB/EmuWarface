using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Items
{
    public partial class Item
    {
        private int _totalDurabilityPoints;
        private int _durabilityPoints;
        private int _quantity;
        private long _expirationTimeUtc;

        public ulong ProfileId          { get; private set; }
        public ulong Id                 { get; private set; }
        public string Name              { get; private set; }
        public ItemType Type            { get; private set; }
        public string Config            { get; set; } = string.Empty;
        public byte AttachedTo          { get; set; }
        public int Slot                 { get; set; }
        public int Equipped             { get; set; }
        public bool ExpiredConfirmed    { get; set; }
        public long BuyTimeUtc          { get; set; }
        public long ExpirationTimeUtc
        {
            get
            {
                if (Type != ItemType.Expiration)
                    throw new InvalidOperationException();
                return _expirationTimeUtc;
            }
            set => _expirationTimeUtc = value;
        }
        public int TotalDurabilityPoints
        {
            get
            {
                if (Type != ItemType.Permanent)
                    throw new InvalidOperationException();
                return _totalDurabilityPoints;
            }
            set => _totalDurabilityPoints = value;
        }
        public int DurabilityPoints
        {
            get
            {
                if (Type != ItemType.Permanent)
                    throw new InvalidOperationException();
                return _durabilityPoints;
            }
            set => _durabilityPoints = value;
        }
        public int Quantity
        {
            get
            {
                if (Type != ItemType.Consumable)
                    throw new InvalidOperationException();
                return _quantity;
            }
            set => _quantity = value;
        }

        public long SecondsLeft
        {
            get
            {
                return Math.Clamp(ExpirationTimeUtc - DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0, long.MaxValue);
            }
        }

        private Item()
        {

        }

        public static Item DefaultItem(string name)
        {
            return new Item
            {
                Name = name,
                Type = ItemType.Default
            };
        }

        public static Item BasicItem(string name)
        {
            return new Item
            {
                Name = name,
                Type = ItemType.Basic
            };
        }

        public static Item ExpirationItem(string name, long seconds)
        {
            return new Item
            {
                Name = name,
                Type = ItemType.Expiration,
                BuyTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpirationTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + seconds
            };
        }

        public static Item PermanentItem(string name, int totalDurabilityPoints, int durabilityPoints)
        {
            return new Item
            {
                Name = name,
                Type = ItemType.Permanent,
                BuyTimeUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                TotalDurabilityPoints = totalDurabilityPoints,
                DurabilityPoints = durabilityPoints
            };
        }

        public static Item ConsumableItem(string name, int quantity)
        {
            return new Item
            {
                Name = name,
                Type = ItemType.Consumable,
                Quantity = quantity
            };
        }

        public void Update(byte attached_to, string config, int slot)
        {
            SetSlot(slot);

            AttachedTo  = attached_to;
            Config      = config;

            Update();
        }

        public void ConsumeItem(int quantity)
        {
            Client client = null;
            lock (Server.Clients)
            {
                client = Server.Clients.FirstOrDefault(x => x.ProfileId == ProfileId);
            }

            if (Type != ItemType.Consumable)
                throw new InvalidOperationException();

            if (Id == 0)
                throw new InvalidOperationException();

            Quantity -= quantity;
            if (Quantity <= 0)
            {
                Quantity = 0;

                if(client != null)
                    GetExpiredItems(client, client.Profile.Items, true);
            }
            else
            {
                Update();

                var expired_items = Xml.Element("get_expired_items")
                    .Child(Serialize("consumable_item"));

                client?.QueryGet(expired_items);
            }
        }


        //TODO
        public void SetSlot(Class @class, ItemSlot slot)
        {
            int[] _slotForClass = new int[5];

            if ((@class & Class.Rifleman) == Class.Rifleman)
            {
                _slotForClass[0] = (int)slot;
            }
            if ((@class & Class.Heavy) == Class.Heavy)
            {
                _slotForClass[1] = (int)slot;
            }
            if ((@class & Class.Recon) == Class.Recon)
            {
                _slotForClass[2] = (int)slot;
            }
            if ((@class & Class.Medic) == Class.Medic)
            {
                _slotForClass[3] = (int)slot;
            }
            if ((@class & Class.Engineer) == Class.Engineer)
            {
                _slotForClass[4] = (int)slot;
            }

            /*foreach (var property in slotForClass)
            {
                _slotForClass[(int)property.Key] = (int)property.Value;
            }*/
            SetSlot(_slotForClass);
        }

        /*public void SetSlot(Dictionary<Class, ItemSlot> slotForClass)
        {
            int[] _slotForClass = new int[5];
            foreach (var property in slotForClass)
            {
                _slotForClass[(int)property.Key] = (int)property.Value;
            }
            SetSlot(_slotForClass);
        }*/

        public void SetSlot(int[] slotForClass)
        {
            SetSlot(slotForClass[0] & 0x3F | ((slotForClass[1] & 0x3F | ((((slotForClass[3] & 0x3F | ((slotForClass[4] & 0x3F) << 6)) << 6) | slotForClass[2] & 0x3F) << 6)) << 6) | 0x40000000);
        }

        /*public void SetSlot(BaseSlot type, IEnumerable<Class> classes)
        {
            Slot = 0;
            foreach (Class @class in classes)
            {
                Slot += (int)Math.Pow(1 << (int)@class, 5) * (int)type;
            }
            SetSlot(Slot);
        }*/

        public void SetSlot(int slotsValue)
        {
            Slot        = slotsValue;
            Equipped    = 0;

            int classes = Convert.ToInt32((slotsValue & 0x40000000) != 0) + 5;
            int types   = 63;

            if (!Convert.ToBoolean(slotsValue & 0x40000000))
                types = 31;

            int[] slotForClass = new int[5]
            {
                slotsValue & types,
                (slotsValue >> classes) & types,
                slotsValue >> classes >> classes & types,
                slotsValue >> classes >> classes >> classes & types,
                types & (slotsValue >> classes >> classes >> classes >> classes)
            };

            for (int i = 0; i < slotForClass.Length; i++)
            {
                if (slotForClass[i] != 0)
                    Equipped += 1 << i;
            }
        }

        public Class GetClass() => (Class)Equipped;

        //TODO
        public ItemSlot GetSlot()
        {
            int classes = Convert.ToInt32((Slot & 0x40000000) != 0) + 5;

            int types = 63;
            if (!Convert.ToBoolean(Slot & 0x40000000))
                types = 31;

            int[] slotForClass =
            {
                Slot & types,
                (Slot >> classes) & types,
                Slot >> classes >> classes & types,
                Slot >> classes >> classes >> classes & types,
                types & (Slot >> classes >> classes >> classes >> classes)
            };

            for (int i = 0; i < slotForClass.Length; i++)
            {
                if (slotForClass[i] != 0)
                    return (ItemSlot)slotForClass[i];
            }

            return 0;
        }

        public void Give(ulong profile_id)
        {
            if (profile_id == 0)
                throw new InvalidOperationException();

            MySqlCommand cmd = new MySqlCommand("INSERT INTO emu_items (`profile_id`, `name`, `config`, `attached_to`, `slot`, `equipped`, `expired_confirmed`, `buy_time_utc`, `expiration_time_utc`, `total_durability_points`, `durability_points`, `quantity`, `type`) VALUES " +
                $"(@profile_id, @name, @config, @attachedTo, @slot, @equipped, @expiredConfirmed, @buyTimeUtc, @expirationTimeUtc, @totalDurabilityPoints, @durabilityPoints, @quantity, @type); SELECT LAST_INSERT_ID();");

            cmd.Parameters.AddWithValue("@profile_id",              profile_id);
            cmd.Parameters.AddWithValue("@name",                    Name);
            cmd.Parameters.AddWithValue("@config",                  Config);
            cmd.Parameters.AddWithValue("@attachedTo",              AttachedTo);
            cmd.Parameters.AddWithValue("@slot",                    Slot);
            cmd.Parameters.AddWithValue("@equipped",                Equipped);
            //cmd.Parameters.AddWithValue("@expiredConfirmed",        /*Convert.ToByte(*/ExpiredConfirmed/*)*/);
            cmd.Parameters.AddWithValue("@expiredConfirmed",        ExpiredConfirmed); //TODO test
            cmd.Parameters.AddWithValue("@buyTimeUtc",              BuyTimeUtc);
            cmd.Parameters.AddWithValue("@expirationTimeUtc",       _expirationTimeUtc);
            cmd.Parameters.AddWithValue("@totalDurabilityPoints",   _totalDurabilityPoints);
            cmd.Parameters.AddWithValue("@durabilityPoints",        _durabilityPoints);
            cmd.Parameters.AddWithValue("@quantity",                _quantity);
            cmd.Parameters.AddWithValue("@type",                    (int)Type);

            Id = (ulong)SQL.QueryRead(cmd).Rows[0][0];
            ProfileId = profile_id;
        }

        public void Update()
        {
            if (ProfileId == 0)
                throw new InvalidOperationException();

            if (Id == 0)
                throw new InvalidOperationException();
            /*{
                Log.Error($"У предмета {Name} ID=0");
                return;
            }*/

            MySqlCommand cmd = new MySqlCommand("UPDATE emu_items SET config=@config, attached_to=@attached_to, slot=@slot, equipped=@equipped, expired_confirmed=@expired_confirmed, expiration_time_utc=@expiration_time_utc, total_durability_points=@total_durability_points, durability_points=@durability_points, quantity=@quantity WHERE id=@id AND profile_id=@profile_id");

            cmd.Parameters.AddWithValue("@id",                      Id);
            cmd.Parameters.AddWithValue("@profile_id",              ProfileId);
            cmd.Parameters.AddWithValue("@config",                  Config);
            cmd.Parameters.AddWithValue("@attached_to",             AttachedTo);
            cmd.Parameters.AddWithValue("@slot",                    Slot);
            cmd.Parameters.AddWithValue("@equipped",                Equipped);
            //cmd.Parameters.AddWithValue("@expired_confirmed",       Convert.ToByte(ExpiredConfirmed));
            cmd.Parameters.AddWithValue("@expired_confirmed",       ExpiredConfirmed);
            cmd.Parameters.AddWithValue("@expiration_time_utc",     _expirationTimeUtc);
            cmd.Parameters.AddWithValue("@total_durability_points", _totalDurabilityPoints);
            cmd.Parameters.AddWithValue("@durability_points",       _durabilityPoints);
            cmd.Parameters.AddWithValue("@quantity",                _quantity);

            SQL.Query(cmd);
        }

        public void Delete(bool resync_profile = false)
        {
            if (ProfileId == 0)
                throw new InvalidOperationException();

            if (Id == 0)
                throw new InvalidOperationException();

            SQL.Query($"DELETE FROM emu_items WHERE id={Id} AND profile_id={ProfileId};");
        }

        public XmlElement Serialize(string name = "item")
        {
            XmlElement node = Xml.Element(name)
                .Attr("id",                 Id)
                .Attr("name",               Name)
                .Attr("attached_to",        AttachedTo)
                .Attr("config",             Config)
                .Attr("slot",               Slot)
                .Attr("equipped",           Equipped)
                .Attr("default",            Convert.ToByte(Type == ItemType.Default))
                .Attr("permanent",          Convert.ToByte(Type == ItemType.Permanent))
                .Attr("expired_confirmed",  Convert.ToByte(ExpiredConfirmed))
                .Attr("buy_time_utc",       BuyTimeUtc);

            switch (Type)
            {
                case ItemType.Basic:
                    break;
                case ItemType.Default:
                    node.Attr("seconds_left", 0);
                    break;
                case ItemType.Consumable:
                    node.Attr("quantity", Quantity);
                    break;
                case ItemType.Permanent:
                    node.Attr("total_durability_points", TotalDurabilityPoints);
                    node.Attr("durability_points", DurabilityPoints);
                    break;
                case ItemType.Expiration:
                    node.Attr("expiration_time_utc", ExpirationTimeUtc);
                    node.Attr("seconds_left", SecondsLeft);
                    break;
            }

            return node;
        }

        public static List<XmlElement> GetExpiredItems(Client client, List<Item> items = null, bool notify = false)
        {
            var expired = GetExpiredItems(client.ProfileId, items);

            if (notify)
            {
                XmlElement get_expired_items = Xml.Element("get_expired_items");
                expired.ForEach(x => get_expired_items.Child(x));

                //TODO k01.warface test
                //Iq res = new Iq(IqType.Get, client.Jid, client.Channel.Jid);
                //client.QueryGet(res.SetQuery(get_expired_items));
                client.QueryGet(get_expired_items);
            }

            return expired;
        }

        public static List<XmlElement> GetExpiredItems(ulong profile_id, List<Item> items = null)
        {
            //<get_expired_items>
            //  <durability_item id='1274591757' name='smg12_shop' attached_to='0' config='' slot='0' equipped='0' default='0' permanent='1' expired_confirmed='0' buy_time_utc='1549284970' total_durability_points='36000' durability_points='36000'/>
            //  <consumable_item id='1274596219' name='smokegrenade04_c' attached_to='0' config='dm=0;material=;pocket_index=1082369' slot='23812118' equipped='29' default='0' permanent='0' expired_confirmed='0' buy_time_utc='1549285229' quantity='10'/>
            //</get_expired_items>

            if (profile_id == 0)
                throw new InvalidOperationException();

            if (items == null)
                items = GetItems(profile_id);

            List<XmlElement> expired_items = new List<XmlElement>();

            foreach (var item in items)
            {
                string name = string.Empty;

                switch (item.Type)
                {
                    case ItemType.Expiration:
                        {
                            if (item.SecondsLeft != 0)
                            {
                                item.ExpiredConfirmed = false;
                                continue;
                            }
                            name = "expired_item";
                            break;
                        }
                    case ItemType.Permanent:
                        {
                            if (item.DurabilityPoints != 0)
                            {
                                item.ExpiredConfirmed = false;
                                continue;
                            }
                            name = "durability_item";
                            break;
                        }
                    case ItemType.Consumable:
                        {
                            if (item.Quantity != 0)
                            {
                                item.ExpiredConfirmed = false;
                                continue;
                            }
                            name = "consumable_item";
                            break;
                        }
                    default: continue;
                }

                if (item.ExpiredConfirmed)
                    continue;

                expired_items.Add(Xml.Element(name)
                    .Attr("id", item.Id)
                    .Attr("name", item.Name)
                    .Attr("slot_ids", item.Slot));

                item.ExpiredConfirmed = true;

                if (item.Equipped != 0)
                {
                    var slot = item.GetSlot();
                    var @class = item.GetClass();

                    Item new_item = null;

                    foreach (var default_item in EmuConfig.DefaultItems)
                    {
                        if ((default_item.Classes & @class) == @class && default_item.Type == slot)
                        {
                            new_item = items.FirstOrDefault(x => x.Name == default_item.Name);
                        }
                    }

                    if (new_item != null)
                    {
                        var new_item_class = new_item.GetClass();
                        var item_class = item.GetClass();

                        new_item.SetSlot(item.GetClass() | new_item.GetClass(), slot);
                        new_item.Update();
                    }

                    item.SetSlot(0);
                }

                item.Update();
            }

            return expired_items;
        }

        public static List<Item> GetItems(ulong profile_id)
        {
            List<Item> items = new List<Item>();
            var db = SQL.QueryRead($"SELECT * FROM emu_items WHERE profile_id={profile_id}");

            foreach (DataRow row in db.Rows)
            {
                items.Add(ParseDataRow(row));
            }

            return items;
        }

        public static Item GetItem(ulong profile_id, ulong item_id)
        {
            var db = SQL.QueryRead($"SELECT * FROM emu_items WHERE profile_id={profile_id} AND id={item_id}");

            if (db.Rows.Count == 0)
                throw new KeyNotFoundException();

            return ParseDataRow(db.Rows[0]);
        }

        private static Item ParseDataRow(DataRow row)
        {
            return new Item
            {
                ProfileId               = (ulong)row["profile_id"],
                Id                      = (ulong)row["id"],
                Name                    = (string)row["name"],
                Config                  = (string)row["config"],
                AttachedTo              = (byte)row["attached_to"],
                Slot                    = (int)row["slot"],
                Equipped                = (int)row["equipped"],
                ExpiredConfirmed        = (bool)row["expired_confirmed"],
                BuyTimeUtc              = (long)row["buy_time_utc"],
                _expirationTimeUtc      = (long)row["expiration_time_utc"],
                _totalDurabilityPoints  = (int)row["total_durability_points"],
                _durabilityPoints       = (int)row["durability_points"],
                _quantity               = (int)row["quantity"],
                Type                    = (ItemType)(byte)row["type"]
            };
        }
    }
}
