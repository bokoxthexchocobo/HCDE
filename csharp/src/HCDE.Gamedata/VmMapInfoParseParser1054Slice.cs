namespace HCDE.Gamedata;

/// <summary>
/// <c>cluster_info_t::Reset</c> and <c>FMapInfoParser</c> brace/assign/number helpers from <c>g_mapinfo.cpp</c> (**451–600**).
/// Chunk **1054**; follows **1053** (**301–450**). Uncredited <see cref="VmMapInfoParserFormat"/> — seam only, not re-credited.
/// </summary>
public static class VmMapInfoParseParser1054Slice
{
    public const int FmtOld = VmMapInfoParseMapInfo1029Slice.FmtOld;
    public const int FmtNew = VmMapInfoParseMapInfo1029Slice.FmtNew;

    public static void ApplyClusterInfoReset(VmMapInfoClusterReset1054 cluster)
    {
        cluster.Cluster = 0;
        cluster.FinaleFlat = string.Empty;
        cluster.ExitText = string.Empty;
        cluster.EnterText = string.Empty;
        cluster.MessageMusic = string.Empty;
        cluster.MusicOrder = 0;
        cluster.Flags = 0;
        cluster.CdTrack = 0;
        cluster.ClusterName = string.Empty;
        cluster.CdId = 0;
    }

    public static int ResolveFormatAfterOpenBrace(int currentFormat, bool sawOpenBraceToken, ref bool enableCMode)
    {
        switch (currentFormat)
        {
            case FmtOld:
                return FmtOld;
            case FmtNew:
                enableCMode = true;
                return FmtNew;
            default:
                var resolved = sawOpenBraceToken ? FmtNew : FmtOld;
                if (resolved == FmtNew)
                    enableCMode = true;
                return resolved;
        }
    }

    public static bool RequiresMustGetOpenBrace(int currentFormat) => currentFormat == FmtNew;

    public static bool TryParseCloseBrace(int formatType, bool tokenIsCloseBrace, out bool shouldUnget)
    {
        shouldUnget = false;
        if (formatType == FmtNew)
            return tokenIsCloseBrace;
        shouldUnget = true;
        return true;
    }

    public static bool CheckAssign(int formatType, bool tokenIsEquals) =>
        formatType == FmtNew && tokenIsEquals;

    public static bool RequiresParseAssign(int formatType, bool tokenIsEquals) =>
        formatType == FmtNew && tokenIsEquals;

    public static bool MustParseAssignOrError(int formatType, bool tokenIsEquals) =>
        formatType == FmtNew ? tokenIsEquals : false;

    public static bool RequiresParseComma(int formatType, bool tokenIsComma) =>
        formatType == FmtNew && tokenIsComma;

    public static bool TryCheckNumber(int formatType, bool tokenIsComma, bool numberConsumed, out bool needsMustGetNumber)
    {
        needsMustGetNumber = false;
        if (formatType == FmtNew)
        {
            if (!tokenIsComma)
                return false;
            needsMustGetNumber = true;
            return true;
        }

        return numberConsumed;
    }

    public static bool TryCheckFloat(int formatType, bool tokenIsComma, bool floatConsumed, out bool needsMustGetFloat)
    {
        needsMustGetFloat = false;
        if (formatType == FmtNew)
        {
            if (!tokenIsComma)
                return false;
            needsMustGetFloat = true;
            return true;
        }

        return floatConsumed;
    }
}

public sealed class VmMapInfoClusterReset1054
{
    public int Cluster { get; set; }
    public string FinaleFlat { get; set; } = "";
    public string ExitText { get; set; } = "";
    public string EnterText { get; set; } = "";
    public string MessageMusic { get; set; } = "";
    public int MusicOrder { get; set; }
    public int Flags { get; set; }
    public int CdTrack { get; set; }
    public string ClusterName { get; set; } = "";
    public int CdId { get; set; }
}
