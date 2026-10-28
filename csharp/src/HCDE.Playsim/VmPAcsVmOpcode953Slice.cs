namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> HUD message tail and gameplay/network PCDs from <c>p_acs.cpp</c> (**9195–9344**).
/// Exclusive to chunk **953**; <c>p_acs.cpp</c> **11250–11388** (139 lines) deferred sub-150 at EOF.
/// </summary>
public static class VmPAcsVmOpcode953Slice
{
    public const int HudMsgLog = unchecked((int)0x80000000);
    public const int HudMsgColorString = unchecked((int)0x40000000);
    public const int HudMsgAddBlend = unchecked((int)0x20000000);
    public const int HudMsgAlpha = unchecked((int)0x10000000);
    public const int HudMsgNoWrap = unchecked((int)0x08000000);
    public const int HudMsgLayerMask = 0x0000F000;
    public const int HudMsgLayerShift = 12;
    public const int HudMsgVisibilityMask = 0x00070000;
    public const int HudMsgVisibilityShift = 16;

    public const int GameNetCooperative = 1;
    public const int GameNetDeathmatch = 2;
    public const int GameTitleMap = 3;

    public const int HudMessageSubtypeMask = 0xFF;
    public const int HudMessageMandatoryOperands = 6;
    public const int EndHudMessageStackResetOperands = 6;
    public const int SetFontStackPop = 1;
    public const int MusicChangeStackPop = 2;

    public static int ClampHudColorIndex(int colorIndex, int numTextColors, int untranslatedColor) =>
        (uint)colorIndex >= (uint)numTextColors ? untranslatedColor : colorIndex;

    public static bool HudMessageUsesColorString(int type) => (type & HudMsgColorString) != 0;

    public static int HudMessageDisplaySubtype(int type) => type & HudMessageSubtypeMask;

    public static float DefaultHudAlpha(int optstart, int sp, int stackTopAlpha) =>
        optstart < sp ? VmPAcsIsClientSideScript.AcsToFloat(stackTopAlpha) : 1f;

    public static (float HoldTime, float FadeTime) FadeOutCompatDuration(float holdTime, float fadeTime, float minFrameSeconds)
    {
        if (holdTime <= 0f && fadeTime <= 0f)
            return (minFrameSeconds, minFrameSeconds);
        return (holdTime, fadeTime);
    }

    public static float FadeOutAlpha(int optstart, int sp, int alphaSlot, int defaultAlpha = 1) =>
        optstart < sp - 1 ? VmPAcsIsClientSideScript.AcsToFloat(alphaSlot) : defaultAlpha;

    public static float TypeOnFadeAlpha(int optstart, int sp, int alphaSlot) =>
        optstart < sp - 2 ? VmPAcsIsClientSideScript.AcsToFloat(alphaSlot) : 1f;

    public static float FadeInOutAlpha(int optstart, int sp, int alphaSlot) =>
        optstart < sp - 2 ? VmPAcsIsClientSideScript.AcsToFloat(alphaSlot) : 1f;

    public static int HudMessageLayer(int type) => (type & HudMsgLayerMask) >> HudMsgLayerShift;

    public static int HudMessageVisibility(int type) =>
        (type & HudMsgVisibilityMask) >> HudMsgVisibilityShift;

    public static bool HudMessageHasFlag(int type, int flag) => (type & flag) != 0;

    public static uint HudMessageAttachId(int id) => id != 0 ? 0xff000000u | (uint)id : 0u;

    public static int HudMessageConsoleColor(int color, int brickColor, int numTextColors, int untranslatedColor) =>
        color >= brickColor && color < numTextColors && color != untranslatedColor
            ? color + 'A'
            : '-';

    public static int EndHudMessageStackPointer(int optstart) => optstart - EndHudMessageStackResetOperands;

    public static int PushGameType(bool titleLevel, bool deathmatch, bool multiplayer)
    {
        if (titleLevel)
            return GameTitleMap;
        if (deathmatch)
            return GameNetDeathmatch;
        if (multiplayer)
            return GameNetCooperative;
        return 0;
    }

    public static int PushPlayerTeam(bool hasActivatorPlayer, int team) =>
        hasActivatorPlayer ? team : 0;

    public static int PushActivatorHealth(object? activator, int health) =>
        activator is not null ? health : 0;

    public static int PushBasicArmorAmount(object? activator, int armorAmount) =>
        activator is not null ? armorAmount : 0;

    public static int PushPlayerFrags(bool hasActivatorPlayer, int fragCount) =>
        hasActivatorPlayer ? fragCount : 0;

    public static bool ShouldChangeMusic(string? lookup) => lookup is not null;

    public static int PushSinglePlayerFlag(bool multiplayer) => multiplayer ? 0 : 1;

    public static int PushNetGameFlag(bool netGame) => netGame ? 1 : 0;
}
