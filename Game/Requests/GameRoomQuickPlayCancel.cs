using EmuWarface.Core;
using EmuWarface.Xmpp;

namespace EmuWarface.Game.Requests
{
    public static class GameRoomQuickPlayCancel
    {
        [Query(IqType.Get, "gameroom_quickplay_cancel")]
        public static void GameRoomQuickPlayCancelSerializer(Client client, Iq iq)
        {
            client.Profile?.Room?.LeftPlayer(client, Enums.RoomPlayerRemoveReason.Left);
        }
    }
}
