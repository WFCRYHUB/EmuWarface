using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class SetCurrentClass
    {
        [Query(IqType.Get, "setcurrentclass")]
        public static void SetCurrentClassSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            client.Profile.CurrentClass = Utils.ParseEnum<ClassId>(q.GetAttribute("current_class"));
            SQL.Query($"UPDATE emu_profiles SET current_class={iq.Query.GetAttribute("current_class")} WHERE profile_id={client.ProfileId}");

            client.Profile.Room?.GetExtension<GameRoomCore>()?.Update();

            iq.SetQuery(Xml.Element("setcurrentclass"));
            client.QueryResult(iq);
        }
    }
}
