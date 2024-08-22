using System;

namespace EmuWarface.Game.Notifications
{
    public enum NotificationType
    {
        Unknown =               1 << 0,
        MissionPerformance =    1 << 1,
        Achivement =            1 << 2,
        Message =               1 << 3,
        ClanInvite =            1 << 4,
        ClanInviteResult =      1 << 5,
        FriendInvite =          1 << 6,
        FriendInviteResult =    1 << 7,
        ItemGiven =             1 << 8,
        Announcement =          1 << 9,
        Contract =              1 << 10,
        MoneyGiven =            1 << 11,
        ItemUnequipped =        1 << 12,
        RandomBoxGiven =        1 << 13,
        ItemUnlocked =          1 << 15,
        AutoRepairEquipment =   1 << 16,
        NewRankReached =        1 << 17,
        CongratulationMessage = 1 << 18,
        MissionUnlocked =       1 << 20,
        ItemDeleted =           1 << 21,
        LeaveGameBan =          1 << 22,
        DeferredSessionReward = 1 << 23,
        DisolveGroup =          1 << 25
    }
}
