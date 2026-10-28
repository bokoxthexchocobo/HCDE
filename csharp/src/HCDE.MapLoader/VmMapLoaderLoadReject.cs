namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadReject</c> from <c>maploader.cpp</c> (2794–2849).</summary>
public static class VmMapLoaderLoadReject
{
    public static int ComputeNeededRejectBytes(int sectorCount) =>
        (sectorCount * sectorCount + 7) >> 3;

    public static bool ShouldClearReject(bool hasRejectLump, int rejectSize, int neededSize) =>
        !hasRejectLump || rejectSize < neededSize;

    public static int ClampReadSize(int rejectSize, int neededSize) =>
        Math.Min(rejectSize, neededSize);

    public static bool ShouldTreatJunkLumpAsEmpty(bool junk) => junk;

    public static int ResolveRejectSizeFromMap(bool hasLump, bool junk, int lumpSize) =>
        !hasLump ? 0 : junk ? 0 : lumpSize;

    public static bool IsRejectMatrixAllZero(ReadOnlySpan<byte> rejectBytes)
    {
        var qwords = rejectBytes.Length / 8;
        for (var i = 0; i < qwords; i++)
        {
            var offset = i * 8;
            ulong word = (ulong)rejectBytes[offset]
                         | ((ulong)rejectBytes[offset + 1] << 8)
                         | ((ulong)rejectBytes[offset + 2] << 16)
                         | ((ulong)rejectBytes[offset + 3] << 24)
                         | ((ulong)rejectBytes[offset + 4] << 32)
                         | ((ulong)rejectBytes[offset + 5] << 40)
                         | ((ulong)rejectBytes[offset + 6] << 48)
                         | ((ulong)rejectBytes[offset + 7] << 56);
            if (word != 0)
                return false;
        }

        var tailStart = qwords * 8;
        for (var i = tailStart; i < rejectBytes.Length; i++)
        {
            if (rejectBytes[i] != 0)
                return false;
        }

        return true;
    }

    public static bool ShouldDiscardAllZeroReject(ReadOnlySpan<byte> rejectBytes) =>
        IsRejectMatrixAllZero(rejectBytes);

    public static int BytesTooSmall(int neededSize, int rejectSize) => neededSize - rejectSize;

    public static bool ShouldPrintRejectTooSmall(int rejectSize) => rejectSize > 0;
}
