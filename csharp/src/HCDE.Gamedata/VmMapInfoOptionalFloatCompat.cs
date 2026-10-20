namespace HCDE.Gamedata;

/// <summary><c>HCDE_MapInfo_CheckOptionalFloatCompat</c> from <c>g_mapinfo.cpp</c> (600–633).</summary>
public static class VmMapInfoOptionalFloatCompat
{
    public static bool TryAcceptOptionalFloat(bool hasFloat) => hasFloat;

    public static bool ShouldAttemptLegacySkySpeedParse(bool newFormat, bool modCompatActive) =>
        newFormat && modCompatActive;

    public static (bool Matched, double Value) TryParseLegacyValue(bool hasFloat, double floatValue, bool hasNumber, int numberValue)
    {
        if (hasFloat)
            return (true, floatValue);
        if (hasNumber)
            return (true, numberValue);
        return (false, 0);
    }

    public static string FormatCompatWarning(string property) =>
        $"HCDE: tolerated MAPINFO '{property}' value without comma for known mod compatibility.";

    public static bool ShouldEmitCompatWarning(ref bool warnedOnce)
    {
        if (warnedOnce)
            return false;
        warnedOnce = true;
        return true;
    }

    public static double ApplyHexenSkySpeedScale(double raw, bool hexenHack) =>
        hexenHack ? raw / 256 : raw;

    public static float SkySpeedFromTics(double scaledValue, double ticRate = 35) =>
        (float)(scaledValue * (ticRate / 1000.0));
}
