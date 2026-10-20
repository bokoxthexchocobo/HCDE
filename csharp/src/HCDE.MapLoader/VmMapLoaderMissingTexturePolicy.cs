namespace HCDE.MapLoader;

/// <summary><c>SetTexture</c> / <c>SummarizeMissingTextures</c> from <c>maploader.cpp</c> (143–235).</summary>
public static class VmMapLoaderMissingTexturePolicy
{
    public const int WarnLimit = 20;

    public static bool ShouldPrintWarning(int count) => count <= WarnLimit;

    public static string TruncateFlatName(string name, bool truncate)
    {
        if (!truncate || name.Length <= 8)
            return name;
        return name[..8];
    }

    public static bool ShouldSummarizeExtraUsage(int count) => count > WarnLimit;

    public static int ExtraUsageCount(int count) => count - WarnLimit;

    public static string FormatSectorMissingMessage(string positionName, string textureName, int sectorIndex) =>
        $"Unknown {positionName} texture '{textureName}' in sector {sectorIndex}";

    public static string FormatWallMissingMessage(
        string positionName,
        string textureName,
        string sideName,
        int lineIndex) =>
        $"Unknown {positionName} texture '{textureName}' on {sideName} side of linedef {lineIndex}";

    public static string FormatSummarizeTail(string textureName, int extraCount) =>
        $"Missing texture '{textureName}' is used {extraCount} more times";

    public static bool ShouldUseDefaultTexture(bool exists) => !exists;

    public static int IncrementMissingCount(int current) => current + 1;
}
