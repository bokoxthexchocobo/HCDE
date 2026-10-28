namespace HCDE.Playsim;

/// <summary><c>ACSStringPool::Dump</c> from <c>p_acs.cpp</c> (1395–1405).</summary>
public static class VmAcsStringPoolDump
{
    public static bool ShouldListEntry(uint next) => next != VmAcsStringPool.FreeEntry;

    public static string FormatEntryLine(uint index, int lockCount, string text) =>
        $"{index,4}. ({lockCount,2}) \"{text}\"";

    public static string FormatFirstFreeLine(uint firstFreeEntry) => $"First free {firstFreeEntry}";
}
