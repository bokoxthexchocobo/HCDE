namespace HCDE.Playsim;

/// <summary><c>PrintMiscActorInfo</c> debug layout helpers from <c>p_mobj.cpp</c> (8987–9061).</summary>
public static class VmPMobjPrintMiscActorInfo
{
    public const int StyleCount = 14;
    public const int StyleNone = 0;

    public static bool QueryNull(bool queryNull) => queryNull;

    public static int MatchLegacyRenderStyle(bool matched, int style) => matched ? style : StyleCount;

    public static bool FlagBitSet(int flags, int bit) => (flags & (1 << bit)) != 0;

    public static bool RenderStyleInTable(int style) => (uint)style < StyleCount;

    public static bool UseLineSpecialName(bool specFound) => specFound;

    public static bool TargetNamePresent(bool hasTarget) => hasTarget;

    public static bool LastEnemyPresent(bool hasLastEnemy) => hasLastEnemy;

    public static int CountFlagBits(int value, int maxBit)
    {
        var count = 0;
        for (var i = 0; i <= maxBit; i++)
        {
            if ((value & (1 << i)) != 0)
                count++;
        }

        return count;
    }

    public readonly record struct MiscInfoHeader(
        bool QueryPresent,
        int LegacyStyle,
        int RenderStyle,
        bool LineSpecialFound);

    public static bool ShouldPrint(MiscInfoHeader h) => h.QueryPresent;

    private static readonly string[] LegacyRenderStyleNames =
    {
        "None", "Normal", "Fuzzy", "SoulTrans", "OptFuzzy", "Stencil", "Translucent",
        "Add", "Shaded", "TranslucentStencil", "Shadow", "Subtract", "AddStencil", "AddShaded"
    };

    public static string RenderStyleLabel(int querystyle) =>
        RenderStyleInTable(querystyle) ? LegacyRenderStyleNames[querystyle] : "Custom";

    public static int ScanLegacyRenderStyle(ReadOnlySpan<int> legacyStyles, int renderStyle)
    {
        for (var style = StyleNone; style < StyleCount; style++)
        {
            if (style < legacyStyles.Length && legacyStyles[style] == renderStyle)
                return style;
        }

        return StyleCount;
    }

    public static bool LineSpecialUsesName(bool specFound, string name) =>
        specFound && name.Length > 0;
}
