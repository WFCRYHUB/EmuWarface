namespace EmuWarface.Core
{
    public interface ICmd
    {
        Permission MinPermission { get; }
        string Usage { get; }
        string Example { get; }
        string[] Names { get; }
        string OnCommand(Permission permission, string[] args);
    }
}
