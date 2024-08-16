namespace EmuWarface.Game.Enums
{
    public enum RoomPlayerRemoveReason
    {
        Left                            = 0,
        KickMaster                      = 1,
        KickTimeout                     = 2,
        KickVote                        = 3,
        KickAdmin                       = 4,
        KickOverflow                    = 5,
        KickRankRestricted              = 6,
        KickClan                        = 7,
        KickAntiCheat                   = 8,
        KickVersionMismatch             = 9,
        KickItemNotAvalaible            = 10,
        KickRankedGameCouldnotStart     = 11,
        KickRankedSessionEnded          = 12,
        KickHighLatency                 = 13,
        KickLostConnection              = 14,
        KickEACOther                    = 15,
        KickEACAuthenticationFailed     = 16,
        KickEACBanned                   = 17,
        KickEACViolation                = 18,
        KickRestrictedEquipment         = 19,
        KickInsufficientPlayers         = 20
    }
    }