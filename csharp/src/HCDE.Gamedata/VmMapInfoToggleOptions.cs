namespace HCDE.Gamedata;

/// <summary>Boolean MAPINFO options with optional assignment from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoToggleOptions
{
    public static bool ParseOptionalBool(bool hasAssign, int numberValue, bool flagWhenOmitted) =>
        hasAssign ? numberValue != 0 : flagWhenOmitted;

    public static void ApplyEdName(VmMapInfoLevelInfo info, string value) => info.EdName = value;

    public static void ApplyAcsName(VmMapInfoLevelInfo info, string value) => info.AcsName = value;
}
