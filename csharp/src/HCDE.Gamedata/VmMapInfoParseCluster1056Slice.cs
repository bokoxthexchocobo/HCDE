namespace HCDE.Gamedata;

/// <summary>
/// `ParseCluster` tail, Hexen exit remap, `ParseNextMap`, shadow cap normalize, and map-option head from <c>g_mapinfo.cpp</c> (**983–1132**).
/// Chunk **1056**; follows **1027** (**833–982**). Uncredited <see cref="VmMapInfoClusterExitTextRemap"/> / <see cref="VmMapInfoParseNextMap"/> / <see cref="VmMapInfoShadowQualityCap"/> — seam only.
/// </summary>
public static class VmMapInfoParseCluster1056Slice
{
    private static readonly int[] ShadowQualityCapSteps = [8192, 4096, 2048, 1024, 512, 256, 128];

    public static bool ApplyClusterCutsceneSlot(
        VmMapInfoClusterParse1027Slice.ClusterDef cluster,
        string keyword,
        VmMapInfoClusterParse1027Slice.CutsceneDef payload) =>
        VmMapInfoClusterParse1027Slice.ApplyClusterProperty(
            cluster,
            keyword,
            stringValue: "",
            lookupName: false,
            musicOrder: 0,
            cutscenePayload: payload,
            matchesDefaultString: static (_, _) => false);

    public static (bool HandleUnknownProperty, bool BreakLoop) ClusterLoopAfterCloseBraceAttempt(bool parsedCloseBrace) =>
        parsedCloseBrace ? (false, true) : (true, false);

    public static bool TryRemapHexenClusterExitText(
        bool exitTextInLump,
        int lumpIndex,
        string? resourceFileName,
        string exitText,
        Func<string, bool> stringTableExists,
        ref int clusterFlags,
        ref string exitTextStorage)
    {
        if (!exitTextInLump || lumpIndex <= 0 || resourceFileName == null)
            return false;
        if (!resourceFileName.Equals("HEXEN.WAD", StringComparison.OrdinalIgnoreCase)
            && !resourceFileName.Equals("HEXDD.WAD", StringComparison.OrdinalIgnoreCase))
            return false;

        var key = $"TXT_{resourceFileName[..Math.Min(5, resourceFileName.Length)]}_{exitText}";
        if (!stringTableExists(key))
            return false;

        exitTextStorage = key;
        clusterFlags &= ~VmMapInfoClusterParse1027Slice.ClusterExitTextInLump;
        clusterFlags |= VmMapInfoClusterParse1027Slice.ClusterLookupExitText;
        return true;
    }

    public static bool ShouldErrorClusterDefinitionEof(int formatType, bool scannerAtEnd) =>
        formatType == VmMapInfoParseParser1054Slice.FmtNew && scannerAtEnd;

    public static string ResolveNumericNextMapToken(int number, bool hexenHack) =>
        hexenHack ? $"&wt@{number:D2}" : $"MAP{number:D2}";

    public static string ResolveEndSequenceNextMapToken(uint sequenceIndex) =>
        $"enDSeQ{sequenceIndex:x4}";

    public static int NormalizeHcdeShadowQualityCap(int value)
    {
        foreach (var step in ShadowQualityCapSteps)
        {
            if (value >= step)
                return step;
        }

        return 128;
    }

    public enum MapOptionKeyword
    {
        Unknown,
        LevelNum,
        Next,
        LightningSound,
        Author,
        Label,
        SecretNext,
        Secret,
        Cluster,
    }

    public static MapOptionKeyword ClassifyMapOptionKeyword(string keyword)
    {
        if (keyword.Equals("levelnum", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.LevelNum;
        if (keyword.Equals("next", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.Next;
        if (keyword.Equals("lightningsound", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.LightningSound;
        if (keyword.Equals("author", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.Author;
        if (keyword.Equals("label", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.Label;
        if (keyword.Equals("secretnext", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.SecretNext;
        if (keyword.Equals("secret", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.Secret;
        if (keyword.Equals("cluster", StringComparison.OrdinalIgnoreCase))
            return MapOptionKeyword.Cluster;
        return MapOptionKeyword.Unknown;
    }

    public static bool MapOptionRequiresAssignAndNumber(MapOptionKeyword keyword) =>
        keyword is MapOptionKeyword.LevelNum or MapOptionKeyword.Cluster;

    public static bool MapOptionUsesParseNextMap(MapOptionKeyword keyword) =>
        keyword is MapOptionKeyword.Next or MapOptionKeyword.SecretNext or MapOptionKeyword.Secret;

    public static bool MapOptionUsesMustGetString(MapOptionKeyword keyword) =>
        keyword is MapOptionKeyword.LightningSound or MapOptionKeyword.Author or MapOptionKeyword.Label;
}
