namespace HCDE.Gamedata;

/// <summary>EMAPINFO property tail from <c>hcde_emapinfo.cpp</c> (675–824).</summary>
public static class VmEMapInfoPropertyTail
{
    public const int TicRate = 35;

    public static float SkySpeedFromDelta(double raw) => (float)(raw * TicRate / 1000.0);

    public static void ApplyUnevenLight(bool enabled, ref int wallVert, ref int wallHoriz)
    {
        if (!enabled)
        {
            wallVert = 0;
            wallHoriz = 0;
        }
    }

    public static bool IsNoFlatSky2(string sky2) =>
        sky2.Equals("-NOFLAT-", StringComparison.OrdinalIgnoreCase) || sky2.Length == 0;

    public static string FormatReportSummary(
        string path,
        int maps,
        int properties,
        int applied,
        int extraResolved,
        int extraMissing,
        int unsupported,
        int invalid) =>
        $"HCDE: parsed EMAPINFO {path}: maps={maps} properties={properties} applied={applied} extradata={extraResolved} missing={extraMissing} unsupported={unsupported} invalid={invalid}.";
}
