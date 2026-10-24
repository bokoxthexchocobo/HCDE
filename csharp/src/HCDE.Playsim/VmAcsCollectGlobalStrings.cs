namespace HCDE.Playsim;

/// <summary><c>P_CollectACSGlobalStrings</c> from <c>p_acs.cpp</c> (1461–1487).</summary>
public static class VmAcsCollectGlobalStrings
{
    public const int StackSize = 4096;

    public static bool ShouldSkipStack(int sp) => sp == 0;

    public static bool IsCorruptedStackPointer(int sp) => sp < 0 && sp >= StackSize;

    public static void CollectFromStacksNative(
        IEnumerable<int> stackPointers,
        Action<IntPtr, int> markStringArray,
        Func<IntPtr, int, IntPtr> stackBufferAddress,
        Action markLevelVarStrings,
        Action markGlobalVarStrings,
        Action purgeStrings)
    {
        foreach (var sp in stackPointers)
        {
            if (ShouldSkipStack(sp))
                continue;
            if (IsCorruptedStackPointer(sp))
                throw new InvalidOperationException("Corrupted stack pointer in ACS VM");

            markStringArray(stackBufferAddress(IntPtr.Zero, sp), sp);
        }

        markLevelVarStrings();
        markGlobalVarStrings();
        purgeStrings();
    }
}
