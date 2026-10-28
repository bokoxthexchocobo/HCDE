namespace HCDE.Playsim;

/// <summary><c>P_ClearACSVars</c> from <c>p_acs.cpp</c> (1531–1558).</summary>
public static class VmAcsClearACSVars
{
    public static void ClearWorldVarsNative(Action clearWorldVars, Action<int> clearWorldArray)
    {
        clearWorldVars();
        for (var i = 0; i < VmAcsClearACSVarsConstants.NumWorldVars; i++)
            clearWorldArray(i);
    }

    public static void ClearACSVarsNative(
        bool alsoGlobal,
        Action clearWorld,
        Action clearGlobal,
        Action purgeAllStrings,
        Action markGlobalVarStrings,
        Action purgeUnreferencedStrings)
    {
        clearWorld();
        if (alsoGlobal)
        {
            clearGlobal();
            purgeAllStrings();
        }
        else
        {
            markGlobalVarStrings();
            purgeUnreferencedStrings();
        }
    }
}

public static class VmAcsClearACSVarsConstants
{
    public const int NumWorldVars = 32;
    public const int NumGlobalVars = 64;
}
