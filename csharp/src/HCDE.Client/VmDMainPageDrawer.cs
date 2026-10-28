namespace HCDE.Client;

/// <summary><c>D_PageDrawer</c> and <c>D_PageTicker</c> from <c>d_main.cpp</c> (1769–1801).</summary>
public static class VmDMainPageDrawer
{
    public static bool ShouldClearFullscreen(bool pageExists) => true;

    public static bool ShouldDrawPageTexture(bool pageExists) => pageExists;

    public static bool ShouldDrawSubtitle(bool subtitlePresent) => subtitlePresent;

    public static bool ShouldUseGenericUiFont(bool genericUi) => genericUi;

    public static bool ShouldDrawAdvisory(bool advisoryValid) => advisoryValid;

    public static int ResolveAdvisoryY() => 160;

    public static int ResolveAdvisoryX() => 4;

    public static bool ShouldAdvanceDemoOnPageTick(int pageTic) => pageTic < 0;
}
