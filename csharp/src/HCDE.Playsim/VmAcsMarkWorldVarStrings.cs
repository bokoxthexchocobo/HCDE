namespace HCDE.Playsim;

/// <summary><c>P_MarkWorldVarStrings</c> from <c>p_acs.cpp</c> (1429–1436).</summary>
public static class VmAcsMarkWorldVarStrings
{
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
}
