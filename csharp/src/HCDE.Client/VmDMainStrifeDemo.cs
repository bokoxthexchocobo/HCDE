namespace HCDE.Client;

/// <summary><c>D_DoStrifeAdvanceDemo</c> early cases from <c>d_main.cpp</c> (1822–1879).</summary>
public static class VmDMainStrifeDemo
{
    public static bool ShouldUseTeaserVoices(int gameFlags, int giShareware) =>
        (gameFlags & giShareware) != 0;

    public static int ResolveTitlePageTicDuration(int ticRate) => 6 * ticRate;

    public static bool ShouldUseBlankPageForCaseOne() => true;

    public static int ResolveRogueLogoTicDuration(int ticRate) => 10 * ticRate / 35;

    public static int ResolveRgLogoTicDuration(int ticRate) => 4 * ticRate;

    public static bool ShouldHideConsoleOnTitle() => true;

    public static bool ShouldPlayLogoMusic(bool hasLogoMusic) => hasLogoMusic;
}
