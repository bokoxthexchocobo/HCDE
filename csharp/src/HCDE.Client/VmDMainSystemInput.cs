namespace HCDE.Client;

/// <summary>Input CVARs and <c>System_*</c> helpers from <c>d_main.cpp</c> (3318–3476).</summary>
public static class VmDMainSystemInput
{
    public const int GameStrife = 3;
    public const int GiShareware = 1;
    public const int MenuOff = 0;
    public const int MenuOn = 1;
    public const int MenuOnNoPause = 2;
    public const int ConsoleUp = 0;
    public const int ConsoleDown = 1;
    public const int ConsoleFalling = 2;
    public const int GsLevel = 1;
    public const int GsCutscene = 2;
    public const int GsDemoscreen = 3;
    public const int GsTitleLevel = 4;

    public static int ClampMouseCaptureMode(int value) =>
        value < 0 ? 0 : value > 2 ? 2 : value;

    public static bool ShouldStopDebugServer(bool vmDebug) => !vmDebug;

    public static bool ShouldSendCenterviewOnMlookRelease(bool lookspring) => lookspring;

    public static bool StrTableValidForStrifeTeaser(string filter, int gametype, int flags) =>
        gametype == GameStrife && (flags & GiShareware) != 0
        && string.Equals(filter, "strifeteaser", StringComparison.OrdinalIgnoreCase);

    public static bool StrTableValidFilter(string filter, int gametype, int flags, string? gameNameForType) =>
        StrTableValidForStrifeTeaser(filter, gametype, flags)
        || gametype == 0
        || (gameNameForType != null && string.Equals(filter, gameNameForType, StringComparison.OrdinalIgnoreCase));

    public static bool WantGuiCaptureWhenMenuOff(int consoleState, bool chatMode) =>
        consoleState == ConsoleDown || consoleState == ConsoleFalling || chatMode;

    public static bool WantGuiCaptureWhenMenuOn(int menuActive) =>
        menuActive == MenuOn || menuActive == MenuOnNoPause;

    public static bool WantGuiCapture(int menuActive, int consoleState, bool chatMode, bool uiProcessorsWantCapture)
    {
        var wantCapt = menuActive == MenuOff
            ? WantGuiCaptureWhenMenuOff(consoleState, chatMode)
            : WantGuiCaptureWhenMenuOn(menuActive);
        if (!wantCapt && uiProcessorsWantCapture)
            wantCapt = true;
        return wantCapt;
    }

    public static bool WantLeftButtonDemoState(int gameState) =>
        gameState == GsDemoscreen || gameState == GsTitleLevel;

    public static bool ShouldApplyDirectMouseLook(bool menuOff, int consoleState, bool eventConsumed, bool paused) =>
        menuOff && consoleState != ConsoleDown && consoleState != ConsoleFalling && !eventConsumed && !paused;

    public static int ScaleMousePitch(int deltaY, double mPitch) => (int)(deltaY * mPitch * 16.0);

    public static int ScaleMouseYaw(int deltaX, double mYaw) => (int)(deltaX * mYaw * 16.0);

    public static bool ShouldApplyPitchLook(bool mlookButton, bool freelook) => mlookButton || freelook;

    public static bool ShouldApplyYawLook(bool strafeButton, bool lookstrafe) => !strafeButton && !lookstrafe;

    public readonly record struct DirectMouseLookResult(int PitchDelta, int YawDelta, int RemainingX, int RemainingY, bool Consumed);

    public static DirectMouseLookResult ApplyDirectMouseLook(
        int rawX,
        int rawY,
        bool mlookButton,
        bool freelook,
        bool strafeButton,
        bool lookstrafe,
        double mPitch,
        double mYaw)
    {
        var x = rawX;
        var y = rawY;
        var pitch = 0;
        var yaw = 0;
        if (ShouldApplyPitchLook(mlookButton, freelook))
        {
            pitch = ScaleMousePitch(y, mPitch);
            y = 0;
        }
        if (ShouldApplyYawLook(strafeButton, lookstrafe))
        {
            yaw = ScaleMouseYaw(x, mYaw);
            x = 0;
        }
        var consumed = x == 0 && y == 0;
        return new(pitch, yaw, x, y, consumed);
    }

    public static bool ShouldConsumeMouseEventAfterLook(int x, int y) => x == 0 && y == 0;

    public static bool CaptureModeInGame(int mode, int gameState, bool demoplayback, bool paused)
    {
        if (demoplayback || paused)
            return false;
        return mode switch
        {
            0 => gameState == GsLevel,
            1 => gameState == GsLevel || gameState == GsCutscene,
            _ => true,
        };
    }

    public static bool WantNativeMouse(bool requireMouse) => requireMouse;

    public static bool IsNetGame(bool netgame) => netgame;

    public static bool IsSpecialUi(
        bool genericUi,
        bool logVgaFont,
        bool dlgVgaFont,
        int consoleState,
        bool multiplayer,
        int menuActive,
        bool currentMenuIsConversation) =>
        genericUi
        || logVgaFont
        || dlgVgaFont
        || consoleState != ConsoleUp
        || multiplayer
        || (menuActive == MenuOn && !currentMenuIsConversation);

    public static bool DisableTextureFilterWhenSoftware(bool hardwareRenderer) => !hardwareRenderer;

    public static bool DisableAnisotropicFiltering(bool intelMipmapDisabled) => intelMipmapDisabled;

    public static bool ShouldWarnVmDebugRestart() => true;

    public static string VmDebugRestartMessage() =>
        "You must restart " + "HCDE" + " for this change to take effect.";

    public static string VmDebugJitNote() =>
        "Note that enabling the debug server will disable JIT compilation.";
}
