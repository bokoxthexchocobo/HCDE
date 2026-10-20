namespace HCDE.Client;

/// <summary>CVAR flag bits from <c>c_cvars.h</c>.</summary>
public static class VmCvarFlags
{
    public const uint Archive = 1;
    public const uint UserInfo = 1 << 1;
    public const uint ServerInfo = 1 << 2;
    public const uint Latch = 1 << 4;
    public const uint Unsettable = 1 << 5;
    public const uint IsDefault = 1 << 7;
    public const uint Auto = 1 << 8;
    public const uint GlobalConfig = 1 << 10;
    public const uint NoSave = 1 << 12;
    public const uint Mod = 1 << 13;
    public const uint Ignore = 1 << 14;
    public const uint UnsafeContext = 1 << 16;
    public const uint ConfigOnly = 1 << 18;

    public const uint ArchiveMask =
        GlobalConfig | Archive | Mod | Auto | UserInfo | ServerInfo | NoSave | ConfigOnly;
}
