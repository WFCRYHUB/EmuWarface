using EmuWarface.Core;
using EmuWarface.Game.Items;
using EmuWarface.Xmpp;
using System;
using System.Xml;

namespace EmuWarface.Game.Requests
{
	public static class GetExpiredItems
	{
		[Query(IqType.Get, "get_expired_items")]
		public static void GetExpiredItemsSerializer(Client client, Iq iq)
		{
			if (client.Profile == null)
			    throw new InvalidOperationException();

			XmlElement get_expired_items = Xml.Element(iq.Query.LocalName);

			Item.GetExpiredItems(client.ProfileId, client.Profile.Items).ForEach(item => get_expired_items.Child(item));

			client.Profile.Items.ForEach(item => get_expired_items.Child(item.Serialize()));

			iq.SetQuery(get_expired_items);
			client.QueryResult(iq);
		}
	}
}
