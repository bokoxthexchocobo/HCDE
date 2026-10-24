namespace HCDE.Gamedata;

/// <summary>MAPINFO MapFlagHandlers compat tail from <c>g_mapinfo.cpp</c> (2055–2106).</summary>
public static class VmMapInfoFlagHandlersCompat
{
    public static IReadOnlyList<VmMapInfoFlagHandler> CompatHandlers =>
    [
        new("compat_noid24", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000002),
        new("compat_voodoozombies", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000004),
        new("compat_noacsargcheck", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000008),
        new("compat_novdolllockmsg", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000010),
        new("compat_emulatemikoportals", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000020),
        new("compat_reservedlineflag", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000040),
        new("compat_nofriendlyspawn", VmMapInfoFlagHandlerType.CompatFlag, 0, 0x00000080),
        new("cd_start_track", VmMapInfoFlagHandlerType.EatNext, 0, 0),
        new("cd_end1_track", VmMapInfoFlagHandlerType.EatNext, 0, 0),
    ];

    public static bool TryGetCompatHandler(string name, out VmMapInfoFlagHandler handler)
    {
        foreach (var entry in CompatHandlers)
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
