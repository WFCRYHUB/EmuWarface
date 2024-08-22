using EmuWarface.Core;
using EmuWarface.Game.Enums;

namespace EmuWarface.Xmpp.Query
{
    public static class GameRoomQuickPlayCancel
    {
        [Query(IqType.Get, "gameroom_quickplay_cancel")]
        public static void GameRoomQuickPlayCancelSerializer(Client client, Iq iq)
        {
            client.Profile?.Room?.LeftPlayer(client, RoomPlayerRemoveReason.Left);
        }
    }
}
