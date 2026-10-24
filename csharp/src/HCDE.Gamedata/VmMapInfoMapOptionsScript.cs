namespace HCDE.Gamedata;

/// <summary>MAPINFO <c>edata</c>, <c>loadacs</c>, <c>intro</c>, <c>outro</c> (<c>g_mapinfo.cpp</c> 1892–1914).</summary>
public static class VmMapInfoMapOptionsScript
{
    public static void ApplyEdName(VmMapInfoLevelInfo info, string edName) => info.EdName = edName;

    public static void ApplyAcsName(VmMapInfoLevelInfo info, string acsName) => info.AcsName = acsName;

    public static void ApplyIntroCutscene(VmMapInfoLevelInfo info, string lump) => info.IntroCutscene = lump;

    public static void ApplyOutroCutscene(VmMapInfoLevelInfo info, string lump) => info.OutroCutscene = lump;
}
