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

    public static void ApplyClearFlag(VmMapInfoLevelFlags level, uint bit, uint orMask)
    {
        level.Flags &= ~bit;
        level.Flags |= orMask;
    }

    public static void ApplySetFlag2(VmMapInfoLevelFlags level, uint bit, bool hasAssign, int number, uint orMask)
    {
        if (!hasAssign) level.Flags2 |= bit;
        else if (number != 0) level.Flags2 |= bit;
        else level.Flags2 &= ~bit;
        level.Flags2 |= orMask;
    }

    public static void ApplyClearFlag2(VmMapInfoLevelFlags level, uint bit, uint orMask)
    {
        level.Flags2 &= ~bit;
        level.Flags2 |= orMask;
    }

    public static void ApplyScFlags2(ref uint flags2, uint setMask, uint andMask) =>
        flags2 = (flags2 & andMask) | setMask;

    public static void ApplySetFlag3(VmMapInfoLevelFlags level, uint bit, bool hasAssign, int number, uint orMask)
    {
        if (!hasAssign) level.Flags3 |= bit;
        else if (number != 0) level.Flags3 |= bit;
        else level.Flags3 &= ~bit;
        level.Flags3 |= orMask;
    }

    public static void ApplyClearCompat(VmMapInfoLevelFlags level, uint data1, uint data2)
    {
        level.CompatFlags &= ~data1;
        level.CompatFlags2 &= ~data2;
        level.CompatMask |= data1;
        level.CompatMask2 |= data2;
    }
}
