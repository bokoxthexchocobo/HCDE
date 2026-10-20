namespace HCDE.Client;

/// <summary>
/// <c>D_Render</c>, <c>D_ToggleHud</c>, and dmflags callbacks from <c>d_main.cpp</c> (594–684, 733–767).
/// </summary>
public static class VmDMainRenderDmflags
{
    public const int DfNoFreelook = 1;
    public const int DfNoFov = 2;
    public const int Df2NoAutomap = 1;
    public const int Df2DisallowSpying = 2;
    public const int Df2Chasecam = 4;

    public static bool ShouldEnableDynamicLights(
        bool glLights,
        int vidRenderMode,
        bool rDynLights,
        bool levelHasLights,
        int lightProbeCount) =>
        (glLights && vidRenderMode == 4) || (rDynLights && vidRenderMode != 4) || lightProbeCount > 0;

    public static bool LevelHasDynamicLights(bool policyAllows, bool levelLights, int lightProbeCount) =>
        policyAllows && (levelLights || lightProbeCount > 0);

    public static bool ShouldReinitSkyOnDmflagsChange(int dmflags, int dfNoFreelook) => true;

    public static bool ShouldCenterViewOnDmflagsChange(int dmflags, int dfNoFreelook) =>
        (dmflags & dfNoFreelook) != 0;

    public static bool ShouldBroadcastFov(int dmflags, int dfNoFov, bool isAuthority) =>
        (dmflags & dfNoFov) != 0 && isAuthority;

    public static float ResolveBroadcastFov(float desiredFov) => desiredFov == 0 ? 90f : desiredFov;

    public static bool ShouldStopAutomap(int dmflags2, int df2NoAutomap, bool automapActive) =>
        (dmflags2 & df2NoAutomap) != 0 && automapActive;

    public static bool ShouldRevertSpyCamera(int dmflags2, int df2DisallowSpying, bool cameraIsPlayerMo) =>
        (dmflags2 & df2DisallowSpying) != 0 && !cameraIsPlayerMo;

    public static bool ShouldDisableChasecam(int dmflags2, int df2Chasecam, bool cheatMode, bool chaseCamActive) =>
        (dmflags2 & df2Chasecam) == 0 && cheatMode && chaseCamActive;

    public readonly struct HudToggleState
    {
        public HudToggleState(int screenBlocks, bool drawPlayerSprite, bool showMessages)
        {
            ScreenBlocks = screenBlocks;
            DrawPlayerSprite = drawPlayerSprite;
            ShowMessages = showMessages;
        }

        public int ScreenBlocks { get; }
        public bool DrawPlayerSprite { get; }
        public bool ShowMessages { get; }
    }

    public static HudToggleState CaptureHudState(int screenBlocks, bool drawPlayerSprite, bool showMessages) =>
        new(screenBlocks, drawPlayerSprite, showMessages);

    public static (int screenBlocks, bool drawSprite, bool showMessages) ApplyHudHidden() =>
        (12, false, false);
}
