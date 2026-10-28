namespace HCDE.Playsim;

/// <summary>Player dive sounds and water-level refresh from <c>p_mobj.cpp</c> (5402–5445).</summary>
public static class VmPMobjWaterLevelRefresh
{
    public static void CallPlayDiveOrSurfaceSounds(bool hasVirtual, Action? vmCall)
    {
        if (hasVirtual)
            vmCall?.Invoke();
    }

    public static bool UpdateWaterLevel(bool doSplash, bool hasPlayer, int oldLevel, Action? splashCheck, Action<int>? playDiveSounds, Action? updateDepth)
    {
        if (doSplash)
            splashCheck?.Invoke();
        updateDepth?.Invoke();
        if (hasPlayer)
            playDiveSounds?.Invoke(oldLevel);
        return false;
    }
}
