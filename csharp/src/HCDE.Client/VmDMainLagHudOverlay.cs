namespace HCDE.Client;

/// <summary><c>DrawHCDELagHUD</c> and <c>DrawOverlays</c> policy from <c>d_main.cpp</c> (1113–1174).</summary>
public static class VmDMainLagHudOverlay
{
    public const int CrGreen = 0;
    public const int CrRed = 1;
    public const int CrGold = 2;
    public const int CrWhite = 3;

    public static bool ShouldDrawLagHud(bool netShouldDraw) => netShouldDraw;

    public static int ResolveLagHudColor(bool ticGateStalled, int commandBacklog, int ticRate, int ticDup)
    {
        if (ticGateStalled || commandBacklog > ticRate)
            return CrRed;
        if (commandBacklog > ticDup * 2)
            return CrGold;
        return CrGreen;
    }

    public static bool IsInvasionIdleDisabled(string? invasionState) =>
        invasionState != null && invasionState == "disabled";

    public static int ComputeFirstLineY(int fontHeight) => fontHeight + 2;

    public static int ComputeLineHeight(int fontHeight) => fontHeight + 1;

    public static bool ShouldDrawOverlaysBeforeStats(bool hudToggled) => !hudToggled;
}
