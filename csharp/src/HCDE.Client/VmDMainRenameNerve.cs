namespace HCDE.Client;

/// <summary><c>RenameNerve</c> from <c>d_main.cpp</c> (3096–3164).</summary>
public static class VmDMainRenameNerve
{
    public const int GameDoom = 0;
    public const int NerveVersionCount = 4;

    public static readonly long[] NerveFileSizes = { 3819855, 3821966, 3821885, 4003409 };

    public static bool ShouldRenameNerveMaps(int gametype) => gametype == GameDoom;

    public static int MatchNerveSizeIndex(long fileLength)
    {
        for (var i = 0; i < NerveVersionCount; i++)
        {
            if (fileLength == NerveFileSizes[i])
                return i;
        }

        return -1;
    }

    public static bool MatchesNerveChecksum(int sizeIndex, ReadOnlySpan<byte> digest, ReadOnlySpan<byte> expected) =>
        sizeIndex >= 0
        && sizeIndex < NerveVersionCount
        && digest.Length == 16
        && expected.Length == 16
        && digest.SequenceEqual(expected);

    public static bool ShouldRenameCwil(uint lumpDword) => lumpDword == 0x4C495743; // CWIL

    public static char RenameCwilFirstChar() => 'N';

    public const uint Map00Dword = 0x3050414D; // MAP0

    public static bool ShouldRenameMap00(uint lumpDword) => lumpDword == Map00Dword;

    public static void ApplyMap00Rename(Span<char> nameChars)
    {
        if (nameChars.Length < 8)
            return;
        nameChars[6] = nameChars[4];
        nameChars[5] = '0';
        nameChars[4] = 'L';
    }
}
