namespace HCDE.Client;

/// <summary><c>FindStrifeTeaserVoices</c> from <c>d_main.cpp</c> (3248–3281).</summary>
public static class VmDMainStrifeTeaserVoices
{
    public const int GameStrife = 3;
    public const int GiShareware = 1 << 0;
    public const int NsGlobal = 1;
    public const int NsStrifeVoices = 12;

    public static bool ShouldScanVoices(int gametype, int gameFlags) =>
        gametype == GameStrife && (gameFlags & GiShareware) != 0;

    public static bool IsVoiceLumpPrefix(ReadOnlySpan<char> name) =>
        name.Length >= 3 && name[0] == 'V' && name[1] == 'O' && name[2] == 'C';

    public static bool IsAllDigitsVoiceSuffix(ReadOnlySpan<char> name)
    {
        if (name.Length < 8)
            return false;
        for (var j = 3; j < 8; j++)
        {
            var c = name[j];
            if (c != 0 && !char.IsDigit(c))
                return false;
        }

        return true;
    }

    public static int StrifeVoicesNamespace() => NsStrifeVoices;
}
