using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Enums
{
    [Flags]
    public enum PlayerStatus
    {
        Offline                     = 0,        //Не в сети
        Online                      = 1 << 0,   //В сети
        Logout                      = 1 << 1,   //Вышел
        Away                        = 1 << 2,   //АФК
        InLobby                     = 1 << 3,   //В лобби
        InGameRoom                  = 1 << 4,   //В комнате
        InGame                      = 1 << 5,   //В бою
        InShop                      = 1 << 6,   //В магазине
        InCustomize                 = 1 << 7,   //На складе
        InRatingGame                = 1 << 8,   //На РМ
        InTutorialGame              = 1 << 9,   //В обучении
        BannedInRatingGame          = 1 << 10,
        BannedInPvpAutostartGame    = 1 << 11
    }
}
