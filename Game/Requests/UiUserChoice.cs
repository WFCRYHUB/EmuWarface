using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace EmuWarface.Game.Requests
{
    public static class UiUserChoice
    {
        /*
         * <ui_user_choice>
<choice choice_from="lobby_pvp_game_room" choice_id="join_quickplay_session" choice_result="1" />
</ui_user_choice>
         */

        [Query(IqType.Get, "ui_user_choice")]
        public static void UiUserChoiceSerializer(Client client, Iq iq)
        {
            //TODO
        }
    }
}
