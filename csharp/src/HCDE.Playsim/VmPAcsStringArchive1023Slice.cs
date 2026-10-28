namespace HCDE.Playsim;

/// <summary>
/// ACS string pool dump/unlock, global-string mark/collect head, script label formatting,
/// and <c>P_ClearACSVars</c> branch policy from <c>p_acs.cpp</c> (**1386–1535**). Chunk **1023**.
/// </summary>
public static class VmPAcsStringArchive1023Slice
{
    public const uint FreeEntry = uint.MaxValue;

    public static bool ShouldListPooledString(uint next) => next != FreeEntry;

    public static bool ShouldRemoveLevelLock(int levelNum, int lockValue) => lockValue == levelNum;

    public static int WorldArrayMarkCount(int arrayCount) => arrayCount;

    public static int GlobalArrayMarkCount(int arrayCount) => arrayCount;

    public static bool StackPointerCorrupted(int sp, int stackSize) => sp < 0 && sp >= stackSize;

    public static string FormatScriptPresentation(int script, Func<int, string?> negativeIndexToName)
    {
        if (script < 0)
        {
            var name = negativeIndexToName(-script);
            if (!string.IsNullOrEmpty(name))
                return $"script \"{name}\"";
        }

        return $"script {script}";
    }

    public static bool ClearAlsoGlobal(bool alsoGlobal) => alsoGlobal;

    public static bool PurgeViaGlobalClear(bool alsoGlobal) => alsoGlobal;

    public static bool PurgeViaMarkPass(bool alsoGlobal) => !alsoGlobal;
}
