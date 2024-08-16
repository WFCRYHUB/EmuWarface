using System;

namespace EmuWarface.Game.Enums
{
    [Flags]
    public enum RoomType
    {
        PvE_Private     = 1 << 0,
        PvP_Public      = 1 << 1,
        PvP_ClanWar     = 1 << 2,
        PvP_Autostart   = 1 << 3,
        PvE_Autostart   = 1 << 4,
        PvP_Rating      = 1 << 5,
        PvE             = 1 << 6,
        PvP             = 1 << 7
    }
}
