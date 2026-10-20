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

    public static string RenderStyleLabel(int querystyle) =>
        RenderStyleInTable(querystyle)
            ? querystyle switch
            {
                0 => "None",
                1 => "Normal",
                7 => "Add",
                _ => "Other"
            }
            : "Custom";
}
