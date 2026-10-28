namespace HCDE.Playsim;

/// <summary><c>P_MarkGlobalVarStrings</c> from <c>p_acs.cpp</c> (1444–1451).</summary>
public static class VmAcsMarkGlobalVarStrings
{
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
}
