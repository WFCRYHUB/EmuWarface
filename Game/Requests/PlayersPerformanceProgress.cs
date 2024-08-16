using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;

namespace EmuWarface.Game.Requests
{
    public static class PlayersPerformanceProgress
    {
        /*
<players_performance_progress session_id="56" mission_id="c9c2ceb5-5370-4bb1-8004-3addb47e5a2b" passed_sublevels_count="2">
<stat id="0" value="159925" />
<stat id="1" value="49125" />
<stat id="2" value="1750" />
<stat id="3" value="625" />
<stat id="4" value="3983" />
<stat id="5" value="571" />
</players_performance_progress>
     */

        [Query(IqType.Get, "players_performance_progress")]
        public static void PlayersPerformanceProgressSerializer(Client client, Iq iq)
        {
            //TODO

            
        }
    }
}
