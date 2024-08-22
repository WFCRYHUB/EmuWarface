namespace EmuWarface.Game.Enums
{
    public enum GameServerStatus
    {
        None            = 0,
        Waiting         = 1,
        Playing         = 2,
        PostGame        = 3, 
        Finished        = 4,
        Free            = 5,
        Ready           = 6,
        Failed          = 7,
        Quiting         = 8,
        NodeChanged     = 9,
        Reconnecting    = 10
    }
}
