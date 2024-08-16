using EmuWarface.Core;
using EmuWarface.Game.Clans;
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
    public static class GetProfile
    {
        //<getprofile session_id="1" id="3" />

        [Query(IqType.Get, "getprofile")]
        public static void GetProfileSerializer(Client client, Iq iq)
        {
            if (!client.IsDedicated || client.Dedicated.Room == null)
                throw new InvalidOperationException();

            var q = iq.Query;
            var target_id = ulong.Parse(q.GetAttribute("id"));

            var room = client.Dedicated.Room;

            XmlElement getprofile = Xml.Element("getprofile")
                .Attr("id", target_id);

            Client roomPlayer = null; 

            lock (Server.Clients)
            {
                roomPlayer = Server.Clients.FirstOrDefault(x => x.ProfileId == target_id);
            }

            if (roomPlayer != null)
            {
                XmlElement profile = Xml.Element("profile")
                    .Attr("user_id",    roomPlayer.UserId)
                    .Attr("nickname",   roomPlayer.Profile.Nickname)
                    .Attr("gender",     "male")
                    .Attr("height",     roomPlayer.Profile.Height)
                    .Attr("fatness",    roomPlayer.Profile.Fatness)
                    .Attr("head",       roomPlayer.Profile.Head)
                    .Attr("experience", roomPlayer.Profile.Experience)
                    .Attr("group_id",   roomPlayer.Profile.RoomPlayer.GroupId)
                    .Attr("clanName",   Clan.GetClanName(roomPlayer.Profile.ClanId))
                    .Attr("preset",     "DefaultPreset")
                    .Attr("current_class",      (int)roomPlayer.Profile.CurrentClass)
                    .Attr("unlocked_classes",   "31"/*TODO all classes*/);

                XmlElement skill = Xml.Element("skill")
                    .Attr("type", "Pve")
                    .Attr("value", "MasterServer.GameLogic.SkillSystem.Skill");

                //TODO test
                XmlElement boosts = Xml.Element("boosts")
                    .Attr("xp_boost", "1")
                    .Attr("vp_boost", "0.1")
                    .Attr("gm_boost", "0.1")
                    .Attr("ic_boost", "0")
                    .Attr("is_vip", "0");

                var inventory_slot = room.GetExtension<GameRoomCustomParams>().GetCurrentRestriction("inventory_slot");

                XmlElement items = Xml.Element("items");
                lock (roomPlayer.Profile.Items)
                {
                    foreach (var item in roomPlayer.Profile.Items)
                    {
                        if(item.Slot != 0)
                        {
                            var slot = item.GetSlot();

                            //МОД(1)
                            if (room.ModOnlyPrimary)
                            {
                                if (slot == ItemSlot.Secondary || slot == ItemSlot.Pistol || slot == ItemSlot.Knife || slot == ItemSlot.Skin || slot > ItemSlot.MeleeSkin)
                                    continue;
                            }

                            //all       34326183935
                            //pistols   17678991309
                            //knifes    17678991317
                            switch (inventory_slot)
                            {
                                case "17678991309":
                                    if (slot == ItemSlot.Primary || slot == ItemSlot.Secondary || slot == ItemSlot.Knife || slot > ItemSlot.MeleeSkin)
                                        continue;
                                    break;
                                case "17678991317":
                                    if (slot == ItemSlot.Primary || slot == ItemSlot.Secondary || slot == ItemSlot.Pistol || slot > ItemSlot.MeleeSkin)
                                        continue;
                                    break;
                            }
                        }

                        if(item.Type == ItemType.Consumable || (item.Equipped != 0 && item.Slot != 0))
                            items.Child(item.Serialize());
                    }
                }

                profile.Child(skill);
                profile.Child(boosts);
                profile.Child(items);

                getprofile.Child(profile);
            }

            client.QueryResult(iq.SetQuery(getprofile));
        }
    }
}
