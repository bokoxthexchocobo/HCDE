namespace HCDE.Playsim;

/// <summary>Exit and teleport line specials from <c>p_lnspec.cpp</c> (227–239, 1067–1203).</summary>
public static class VmPLnspecExitTeleport
{
    public const int TelfDestFog = 1;
    public const int TelfFdCompat = 2;
    public const int TelfSourceFog = 4;
    public const int TelfKeepVelocity = 8;
    public const int TelfKeepOrientation = 16;
    public const int TelfRotateBoom = 32;
    public const int TelfRotateBoomInverse = 64;
    public const int TelfKeepHeight = 128;
    public const int GameStrife = 4;

    public static bool PolyStopSoundFound(object? poly) => poly != null;

    public static bool ExitGood(bool check) => check;

    public static bool ExitSecretUsesSecretMap(bool secret) => secret;

    public static bool TeleportNewMapAllowBackside(int gametype) => gametype == GameStrife;

    public static int TeleportFlags(bool noSourceFog) =>
        TelfDestFog | TelfFdCompat | (noSourceFog ? 0 : TelfSourceFog);

    public static int TeleportNoStopFlags(bool noSourceFog) =>
        TelfDestFog | TelfKeepVelocity | (noSourceFog ? 0 : TelfSourceFog);

    public static int TeleportNoFogFlags(int useAng, bool hasLine) => useAng switch
    {
        0 => TelfKeepOrientation,
        2 => hasLine ? TelfKeepOrientation | TelfRotateBoom : 0,
        3 => hasLine ? TelfKeepOrientation | TelfRotateBoomInverse : 0,
        _ => 0,
    };

    public static int TeleportNoFogWithHeight(int baseFlags, bool keepHeight) =>
        keepHeight ? baseFlags | TelfKeepHeight : baseFlags;

    public static bool ZombieChangerNeedsActivator(object? it) => it != null;

    public static bool ZombieChangerPainWhenAlive(int health) => health >= 0;

    public static bool TeleportOtherFog(bool fogArg) => fogArg;

    public static bool TeleportGroupMoveSource(bool arg) => arg;

    public static bool TeleportGroupFog(bool arg) => arg;

    public static bool TeleportSectorFog(bool arg) => arg;

    public static bool TeleportEndGameFrontOnly(bool backSide) => !backSide;

    public static bool TeleportEndGameExitGood(bool good) => good;

    public static bool TeleportLineUsesSilent(bool reversed) => true;

    public static bool TeleportNewMapNeedsInfo(object? info) => info != null;

    public static bool ChangeLevelKeepFacing(int arg) => arg != 0;

    public static int TeleportNoFogDefaultCase(int useAng) => useAng == 1 ? 0 : 0;

    public static bool TeleportOtherReturns(bool result) => result;

    public static bool TeleportGroupReturns(bool result) => result;

    public static bool TeleportSectorReturns(bool result) => result;

    public static bool ExitNormalPosition(int pos) => pos >= 0;

    public static bool ExitSecretPosition(int pos) => pos >= 0;

    public static string ChangeLevelNullMap() => string.Empty;

    public static bool TeleportEndGameChangesLevel(bool good) => good;
}
