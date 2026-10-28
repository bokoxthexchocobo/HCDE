namespace HCDE.Playsim;

/// <summary>
/// ACS string pool dump/unlock, global-string mark/collect head, script label formatting,
/// and <c>P_ClearACSVars</c> branch policy from <c>p_acs.cpp</c> (**1386–1535**). Chunk **1023**.
/// </summary>
public static class VmPAcsStringArchive1023Slice
{
    public const uint FreeEntry = uint.MaxValue;
    public const int StackSize = 4096;
    public const int NumWorldVars = 32;
    public const int NumGlobalVars = 64;

    public static bool ShouldListPooledString(uint next) => next != FreeEntry;

    public static string FormatDumpLine(uint index, int lockCount, string text) =>
        $"{index,4}. ({lockCount,2}) \"{text}\"";

    public static bool ShouldRemoveLevelLock(int levelNum, int lockValue) => lockValue == levelNum;

    public static void UnlockLevelLocksNative(
        ReadOnlySpan<VmAcsStringPoolUnlockForLevel.PoolEntry> pool,
        int levelNum,
        Action<int, int> deleteLockAt)
    {
        for (var i = 0; i < pool.Length; i++)
        {
            if (pool[i].Next == VmAcsStringPool.FreeEntry)
                continue;

            var ndx = VmAcsStringPoolUnlockForLevel.FindLevelLockIndex(pool[i].Locks, levelNum);
            if (ndx < pool[i].Locks.Length)
                deleteLockAt(i, ndx);
        }
    }

    public static void MarkWorldVarStringsNative(
        Action<IntPtr, int> markStringArray,
        IntPtr worldVarsPointer,
        int worldVarsSize,
        ReadOnlySpan<IntPtr> worldArrayPointers,
        Action<IntPtr> markStringMap)
    {
        markStringArray(worldVarsPointer, worldVarsSize);
        for (var i = 0; i < worldArrayPointers.Length; i++)
            markStringMap(worldArrayPointers[i]);
    }

    public static void MarkGlobalVarStringsNative(
        Action<IntPtr, int> markStringArray,
        IntPtr globalVarsPointer,
        int globalVarsSize,
        ReadOnlySpan<IntPtr> globalArrayPointers,
        Action<IntPtr> markStringMap)
    {
        markStringArray(globalVarsPointer, globalVarsSize);
        for (var i = 0; i < globalArrayPointers.Length; i++)
            markStringMap(globalArrayPointers[i]);
    }

    public static bool ShouldSkipStackPointer(int sp) => sp == 0;

    public static bool StackPointerCorrupted(int sp) => sp < 0 && sp >= StackSize;

    public static void CollectGlobalStringsNative(
        IEnumerable<int> stackPointers,
        Func<IntPtr, int, IntPtr> stackBufferAddress,
        Action<IntPtr, int> markStringArray,
        Action markLevelVarStrings,
        Action markGlobalVarStrings,
        Action purgeStrings)
    {
        foreach (var sp in stackPointers)
        {
            if (ShouldSkipStackPointer(sp))
                continue;
            if (StackPointerCorrupted(sp))
                throw new InvalidOperationException("Corrupted stack pointer in ACS VM");

            markStringArray(stackBufferAddress(IntPtr.Zero, sp), sp);
        }

        markLevelVarStrings();
        markGlobalVarStrings();
        purgeStrings();
    }

    public static string FormatScriptPresentation(int script, Func<int, string?> negativeIndexToName)
    {
        if (script < 0)
        {
            var name = negativeIndexToName(-script);
            if (!string.IsNullOrEmpty(name))
                return VmAcsScriptPresentation.FormatNamedScript(name);
        }

        return VmAcsScriptPresentation.FormatScriptNumber(script);
    }

    public static void ClearWorldSideNative(Action fillWorldVars, Action<int> clearWorldArray)
    {
        fillWorldVars();
        for (var i = 0; i < NumWorldVars; i++)
            clearWorldArray(i);
    }

    public static void ClearGlobalSideNative(Action fillGlobalVars, Action<int> clearGlobalArray)
    {
        fillGlobalVars();
        for (var i = 0; i < NumGlobalVars; i++)
            clearGlobalArray(i);
    }

    public static void ClearAcsVarsNative(
        bool alsoGlobal,
        Action clearWorldSide,
        Action clearGlobalSide,
        Action purgeAllStrings,
        Action markGlobalVarStrings,
        Action purgeUnreferencedStrings)
    {
        clearWorldSide();
        if (alsoGlobal)
        {
            clearGlobalSide();
            purgeAllStrings();
        }
        else
        {
            markGlobalVarStrings();
            purgeUnreferencedStrings();
        }
    }
}
