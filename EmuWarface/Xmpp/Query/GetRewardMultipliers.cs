using EmuWarface.Core;
using EmuWarface.Xmpp;

namespace EmuWarface.Xmpp.Query
{
    public static class GetRewardMultipliers
    {
        [Query(IqType.Get, "get_reward_multipliers")]
        public static void GetRewardMultipliersSerializer(Client client, Iq iq)
        {
            iq.SetQuery(Xml.Element("get_reward_multipliers")
                .Attr("money_multiplier", "1")
                .Attr("exp_multiplier", "1")
                .Attr("sp_multiplier", "1")
                .Attr("crown_multiplier", "1"));

            client.QueryResult(iq);
        }
    }
}
