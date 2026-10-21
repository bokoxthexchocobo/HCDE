namespace HCDE.Playsim;

/// <summary><c>A_Warp</c> inventory return priming and destination ref guard (3707–3730).</summary>
public static class VmActionWarp
{
    public const int WarpfUsetid = 0x2000;

    public static bool ShouldPrimeChainResult(bool hasChainSlot) => hasChainSlot;

    public static bool ShouldClearStatePointer(bool hasStateSlot) => hasStateSlot;

    public static bool UseTidLookup(int flags) => (flags & WarpfUsetid) != 0;

    public static bool WarpFailsWithoutReference(object? reference) => reference is null;
}
