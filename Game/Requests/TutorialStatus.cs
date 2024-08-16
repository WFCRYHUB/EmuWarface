using EmuWarface.Core;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Requests
{
    public static class TutorialStatus
    {
        /*<tutorial_status id="678d8734-cc8a-4472-bb87-19bdb40107a8" step="tutorial_started" event="0" />
         */

        [Query(IqType.Get, "tutorial_status")]
        public static void TutorialStatusSerializer(Client client, Iq iq)
        {
            //TODO
        }
    }
}
