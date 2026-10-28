namespace HCDE.Gamedata;

/// <summary>EMAPINFO lump path helpers from <c>hcde_emapinfo.cpp</c> (525–674).</summary>
public static class VmEMapInfoLumpResolve
{
    public static string? TryBuildSiblingRelativePath(string? emapinfoFullName, string value)
    {
        var name = VmEMapInfoLineParser.NormalizeResourceName(value);
        if (name.Length == 0 || emapinfoFullName is null)
            return null;
        var sourcePath = VmEMapInfoLineParser.NormalizeResourceName(emapinfoFullName);
        var slash = sourcePath.LastIndexOf('/');
        if (slash < 0) return null;
        return sourcePath[..(slash + 1)] + name;
    }

    public static bool ShouldTryShortLumpName(string name) =>
        name.Length <= 8 && !VmEMapInfoLineParser.HasPathOrExtension(name);

    public static bool TryApplyLevelNameKey(string key, string value, out string levelName, out bool clearLookup)
    {
        levelName = "";
        clearLookup = false;
        if (key is not ("levelname" or "inter-levelname")) return false;
        levelName = value;
        clearLookup = true;
        return true;
    }

    public static double ScaleFixedGravity(double raw) =>
        raw == 0 ? double.MaxValue : raw / 65536.0;

    public static string NormalizeLumpLookupName(string value) =>
        VmEMapInfoLineParser.NormalizeResourceName(value);
}
