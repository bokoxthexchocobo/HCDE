namespace HCDE.Gamedata;

/// <summary>Warp-trans map resolution from <c>g_mapinfo.cpp</c> (128–334).</summary>
public static class VmMapInfoWarpTrans
{
    public static bool TryResolveWarpTransMap(
        string mapName,
        bool substitute,
        IReadOnlyList<VmMapInfoLevelRecord> levels,
        out string resolved)
    {
        resolved = mapName;
        if (mapName.Length <= 4 || mapName[0] != '&')
            return false;
        if (mapName.Length < 4
            || (char.ToUpperInvariant(mapName[1]) != 'W')
            || (char.ToUpperInvariant(mapName[2]) != 'T')
            || mapName[3] != '@')
            return false;

        if (!int.TryParse(mapName.AsSpan(4), out var trans))
            return false;

        var lev = VmMapInfoLevelLookup.FindLevelByWarpTrans(levels, trans);
        if (lev != null)
        {
            resolved = lev.MapName;
            return true;
        }

        if (substitute && mapName.Length > 5)
        {
            resolved = $"MAP{mapName[4]}{mapName[5]}";
            return true;
        }

        return false;
    }

    public static bool SecretLevelVisited(IReadOnlyList<VmMapInfoVisitState> levels)
    {
        foreach (var lev in levels)
        {
            if (lev.IsSecret && lev.Visited)
                return true;
        }

        return false;
    }
}

public sealed class VmMapInfoVisitState
{
    public bool IsSecret { get; set; }
    public bool Visited { get; set; }
}
