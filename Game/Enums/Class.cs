using System;

namespace EmuWarface.Game.Enums
{
    [Flags]
    public enum Class
    {
        None        = 0,
        Rifleman    = 1 << 0,
        Heavy       = 1 << 1,
        Recon       = 1 << 2,
        Medic       = 1 << 3,
        Engineer    = 1 << 4
    }
}
