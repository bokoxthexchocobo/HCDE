namespace HCDE.Gamedata;

public sealed class VmMapInfoLevelFlags
{
    public uint Flags { get; set; }
    public uint Flags2 { get; set; }
    public uint Flags3 { get; set; }
    public uint CompatFlags { get; set; }
    public uint CompatFlags2 { get; set; }
    public uint CompatMask { get; set; }
    public uint CompatMask2 { get; set; }
}

/// <summary><c>ParseMapDefinition</c> flag handler switch from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoFlagApply
{
    public static void ApplySetFlag(VmMapInfoLevelFlags level, uint bit, bool hasAssign, int number, uint orMask)
    {
        if (!hasAssign) level.Flags |= bit;
        else if (number != 0) level.Flags |= bit;
        else level.Flags &= ~bit;
        level.Flags |= orMask;
    }

    public static void ApplyScFlags(ref uint flags, uint setMask, uint andMask) =>
        flags = (flags & andMask) | setMask;

    public static void ApplyCompat(VmMapInfoLevelFlags level, uint data1, uint data2, bool set)
    {
        if (set)
        {
            level.CompatFlags |= data1;
            level.CompatFlags2 |= data2;
        }
        else
        {
            level.CompatFlags &= ~data1;
            level.CompatFlags2 &= ~data2;
        }

        level.CompatMask |= data1;
        level.CompatMask2 |= data2;
    }
}
