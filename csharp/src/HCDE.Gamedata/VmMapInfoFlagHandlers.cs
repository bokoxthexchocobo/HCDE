namespace HCDE.Gamedata;

public enum VmMapInfoFlagHandlerType
{
    Ignore,
    EatNext,
    SetFlag,
    ClearFlag,
    SetScFlags,
    SetFlag2,
    ClearFlag2,
    SetScFlags2,
    SetFlag3,
    ClearFlag3,
    CompatFlag,
    ClearCompatFlag
}

public readonly record struct VmMapInfoFlagHandler(string Name, VmMapInfoFlagHandlerType Type, uint Data1, uint Data2);

/// <summary><c>MapFlagHandlers</c> catalog from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoFlagHandlers
{
    public static IReadOnlyList<VmMapInfoFlagHandler> Handlers =>
    [
        new("nointermission", VmMapInfoFlagHandlerType.SetFlag, 0x00000001, 0),
        new("intermission", VmMapInfoFlagHandlerType.ClearFlag, 0x00000001, 0),
        new("doublesky", VmMapInfoFlagHandlerType.SetFlag, 0x00000002, 0),
        new("allowjump", VmMapInfoFlagHandlerType.ClearFlag, 0x00000080, 0),
        new("nojump", VmMapInfoFlagHandlerType.SetFlag, 0x00000080, 0),
        new("noinfighting", VmMapInfoFlagHandlerType.SetScFlags, 0x00000004, 0xfffffffb),
        new("totalinfighting", VmMapInfoFlagHandlerType.SetScFlags, 0x00000008, 0xfffffffb),
        new("compat_dehhealth", VmMapInfoFlagHandlerType.CompatFlag, 0x00002000, 0),
        new("compat_nombf21", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000001),
        new("allowmonstertelefrags", VmMapInfoFlagHandlerType.SetFlag, 0x00000004, 0),
        new("dsparilspecial", VmMapInfoFlagHandlerType.SetFlag, 0x00000100, 0),
        new("ironlichspecial", VmMapInfoFlagHandlerType.SetFlag, 0x00000200, 0),
        new("e1m8special", VmMapInfoFlagHandlerType.SetFlag, 0x00000001, 0),
        new("e2m8special", VmMapInfoFlagHandlerType.SetFlag, 0x00000002, 0),
        new("e3m8special", VmMapInfoFlagHandlerType.SetFlag, 0x00000004, 0),
        new("e4m8special", VmMapInfoFlagHandlerType.SetFlag, 0x00000008, 0),
        new("e4m6special", VmMapInfoFlagHandlerType.SetFlag, 0x00000010, 0),
        new("specialaction_exitlevel", VmMapInfoFlagHandlerType.SetScFlags, 0, 0xffffffc0),
        new("specialaction_opendoor", VmMapInfoFlagHandlerType.SetScFlags, 0x00000040, 0xffffffc0),
        new("map07special", VmMapInfoFlagHandlerType.SetFlag, 0x00000008, 0),
        new("baronspecial", VmMapInfoFlagHandlerType.SetFlag, 0x00000010, 0),
        new("cyberdemonspecial", VmMapInfoFlagHandlerType.SetFlag, 0x00000020, 0),
        new("spidermastermindspecial", VmMapInfoFlagHandlerType.SetFlag, 0x00000040, 0),
        new("minotaurspecial", VmMapInfoFlagHandlerType.SetFlag, 0x00000080, 0),
        new("lightning", VmMapInfoFlagHandlerType.SetFlag, 0x00000200, 0),
        new("smoothlighting", VmMapInfoFlagHandlerType.SetFlag2, 0x00000001, 0),
        new("noautosequences", VmMapInfoFlagHandlerType.SetFlag, 0x00000400, 0),
        new("autosequences", VmMapInfoFlagHandlerType.ClearFlag, 0x00000400, 0),
        new("forcenoskystretch", VmMapInfoFlagHandlerType.SetFlag, 0x00000800, 0),
        new("skystretch", VmMapInfoFlagHandlerType.ClearFlag, 0x00000800, 0),
        new("allowfreelook", VmMapInfoFlagHandlerType.SetScFlags, 0x00000001, 0xfffffffe),
        new("nofreelook", VmMapInfoFlagHandlerType.SetScFlags, 0x00000002, 0xfffffffd),
        new("fallingdamage", VmMapInfoFlagHandlerType.SetScFlags, 0x00000004, 0xfffffffb),
        new("oldfallingdamage", VmMapInfoFlagHandlerType.SetScFlags, 0x00000008, 0xfffffffb),
        new("forcefallingdamage", VmMapInfoFlagHandlerType.SetScFlags, 0x00000008, 0xfffffffb),
        new("strifefallingdamage", VmMapInfoFlagHandlerType.SetFlag, 0x0000000c, 0),
        new("nofallingdamage", VmMapInfoFlagHandlerType.SetScFlags, 0, 0xfffffff3),
        new("missilesactivateimpactlines", VmMapInfoFlagHandlerType.SetFlag2, 0x00000020, 0),
        new("teamplayon", VmMapInfoFlagHandlerType.SetScFlags, 0x00000100, 0xfffffeff),
        new("teamplayoff", VmMapInfoFlagHandlerType.SetScFlags, 0x00000200, 0xfffffdff),
        new("resetinventory", VmMapInfoFlagHandlerType.SetFlag2, 0x00000400, 0),
        new("disableshadowmap", VmMapInfoFlagHandlerType.SetFlag3, 0x00000001, 0),
        new("compat_trace", VmMapInfoFlagHandlerType.CompatFlag, 0x00001000, 0),
        new("compat_nombf21", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000001),
        new("compat_noid24", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000002),
        new("noallies", VmMapInfoFlagHandlerType.SetFlag, 0x00001000, 0),
        new("filterstarts", VmMapInfoFlagHandlerType.SetFlag, 0x00002000, 0),
        new("useplayerstartz", VmMapInfoFlagHandlerType.SetFlag, 0x00004000, 0),
        new("randomplayerstarts", VmMapInfoFlagHandlerType.SetFlag2, 0x00000002, 0),
        new("activateowndeathspecials", VmMapInfoFlagHandlerType.SetFlag, 0x00008000, 0),
        new("killeractivatesdeathspecials", VmMapInfoFlagHandlerType.ClearFlag, 0x00008000, 0),
        new("noinventorybar", VmMapInfoFlagHandlerType.SetFlag, 0x00010000, 0),
        new("keepfullinventory", VmMapInfoFlagHandlerType.SetFlag2, 0x00000004, 0),
        new("resetitems", VmMapInfoFlagHandlerType.SetFlag, 0x00000001, 0),
        new("allowcrouch", VmMapInfoFlagHandlerType.ClearFlag, 0x00020000, 0),
        new("nocrouch", VmMapInfoFlagHandlerType.SetFlag, 0x00020000, 0),
        new("normalinfighting", VmMapInfoFlagHandlerType.SetScFlags, 0, 0xfffffffc),
        new("infiniteflightpowerup", VmMapInfoFlagHandlerType.SetFlag2, 0x00000008, 0),
        new("noinfiniteflightpowerup", VmMapInfoFlagHandlerType.ClearFlag2, 0x00000008, 0),
        new("allowrespawn", VmMapInfoFlagHandlerType.SetFlag2, 0x00000010, 0),
    ];

    public static bool TryGetHandler(string name, out VmMapInfoFlagHandler handler)
    {
        foreach (var entry in Handlers)
        {
            if (entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                handler = entry;
                return true;
            }
        }

        handler = default;
        return false;
    }
}
