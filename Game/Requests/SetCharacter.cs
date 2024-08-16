using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using System;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class SetCharacter
    {
        [Query(IqType.Get, "setcharacter")]
        public static void SetCharacterSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var nodes = iq.Query.ChildNodes;

            foreach (XmlElement node in nodes)
            {
                var item = client.Profile.Items.FirstOrDefault(x => x.Id == ulong.Parse(node.GetAttribute("id")));

                if (item == null)
                    throw new QueryException(1);

                int slot            = 0;
                byte attached_to    = 0;

                //byte.TryParse(node.GetAttribute("attached_to"), out attached_to);
                int.TryParse(node.GetAttribute("slot"), out slot);

                item.Update(attached_to, node.GetAttribute("config"), slot);
                //item.Update();
            }

            client.Profile.CurrentClass = EmuExtensions.ParseEnum<ClassId>(iq.Query.GetAttribute("current_class"));

            SQL.Query($"UPDATE emu_profiles SET current_class={iq.Query.GetAttribute("current_class")} WHERE profile_id={client.ProfileId}");

            //return iq.SetQuery(Xml.Element("setcharacter"));
            client.QueryResult(iq.SetQuery(Xml.Element("setcharacter")));
        }
    }
}
