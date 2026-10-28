namespace HCDE.Gamedata;

/// <summary>MAPINFO intermission picture options from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoMapOptionsPics
{
    public static void ApplyExitPic(VmMapInfoLevelOptionsDraft draft, string pic) =>
        draft.NextMap = pic;

    public static void ApplyEnterPic(VmMapInfoLevelOptionsDraft draft, string pic) =>
        draft.AuthorName = pic;

    public static void ApplyInterPic(VmMapInfoLevelOptionsDraft draft, string pic) =>
        draft.MapLabel = pic;

    public static void ApplyHideAuthorFlag(uint flags3, bool hide) =>
        _ = hide ? flags3 | 0x00000002u : flags3 & ~0x00000002u;
}
