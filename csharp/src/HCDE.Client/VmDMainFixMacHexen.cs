namespace HCDE.Client;

/// <summary><c>FixMacHexen</c> from <c>d_main.cpp</c> (3177–3246).</summary>
public static class VmDMainFixMacHexen
{
    public const int GameHexen = 2;
    public const int ExtraLumps = 299;
    public const int BetaMap41LumpReserve = 12;

    public static readonly long DemoSize = 13596228;
    public static readonly long BetaSize = 13749984;
    public static readonly long FullSize = 21078584;

    public static bool ShouldFixMacHexen(int gametype) => gametype == GameHexen;

    public static bool IsKnownMacHexenIwadSize(long iwadSize) =>
        iwadSize == DemoSize || iwadSize == BetaSize || iwadSize == FullSize;

    public static bool IsHexenBetaChecksum(ReadOnlySpan<byte> digest, ReadOnlySpan<byte> betaMd5) =>
        digest.Length == 16 && betaMd5.Length == 16 && digest.SequenceEqual(betaMd5);

    public static bool IsRecognizedMacHexenChecksum(
        ReadOnlySpan<byte> digest,
        ReadOnlySpan<byte> demoMd5,
        ReadOnlySpan<byte> betaMd5,
        ReadOnlySpan<byte> fullMd5)
    {
        if (digest.Length != 16)
            return false;
        return digest.SequenceEqual(demoMd5)
            || digest.SequenceEqual(betaMd5)
            || digest.SequenceEqual(fullMd5);
    }

    public static int ComputeLastLumpIndex(int lastEntry, bool isBeta) =>
        lastEntry - (isBeta ? BetaMap41LumpReserve : 0);

    public static bool HasExtraLumpSlack(int firstEntry, int lastLump) =>
        firstEntry + ExtraLumps < lastLump;

    public static (int start, int end) ExtraLumpDeletionRange(int lastLump) =>
        (lastLump - ExtraLumps + 1, lastLump);
}
