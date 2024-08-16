using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game.Enums.Errors
{
    public enum CreateProfileError
    {
        AlreadyExist                = 1,
        InvalidNickname             = 2,
        ReservedNickname            = 3,
        VersionMismatch             = 4,
        //InternalNicknameCollision   = 7
    }
}
