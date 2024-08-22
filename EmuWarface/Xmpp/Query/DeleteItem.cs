using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Linq;

namespace EmuWarface.Xmpp.Query
{
    public static class DeleteItem
    {
        [Query(IqType.Get, "delete_item")]
        public static void DeleteItemSerializer(Client client, Iq iq)
        {
            if (client.Profile == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            ulong item_id = ulong.Parse(q.GetAttribute("item_id"));
            var item = client.Profile.Items.FirstOrDefault(x => x.Id == item_id);

            if (item == null)
                throw new QueryException(1);

            item.Delete();
            client.Profile.Items.Remove(item);

            iq.SetQuery(Xml.Element("delete_item"));
            client.QueryResult(iq);
        }
    }
}
