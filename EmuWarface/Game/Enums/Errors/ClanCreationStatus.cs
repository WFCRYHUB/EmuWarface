namespace EmuWarface.Game.Enums.Errors
{
    public enum ClanCreationStatus
    {
        Created             = 0,
        NeedBuyItem         = 1,
        InvalidName         = 2,
        CensoredName        = 3,
        DuplicateName       = 4,
        AlreadyClanMember   = 5,
        ServiceError        = 6,
        NameReserved        = 7
    }
}
