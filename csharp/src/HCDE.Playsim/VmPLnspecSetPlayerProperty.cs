namespace HCDE.Playsim;

/// <summary>Native <c>LS_SetPlayerProperty</c> from <c>p_lnspec.cpp</c> (2911–3128).</summary>
public static class VmPLnspecSetPlayerProperty
{
    public const int PropFrozen = 0;
    public const int PropNotarget = 1;
    public const int PropInstantWeaponSwitch = 2;
    public const int PropFly = 3;
    public const int PropTotallyFrozen = 4;
    public const int PropInvulnerability = 5;
    public const int PropSpeed = 15;
    public const int PropBuddha = 16;
    public const int PropBuddha2 = 17;
    public const int PropFrightening = 18;
    public const int PropNoclip = 19;
    public const int PropNoclip2 = 20;
    public const int PropGodmode = 21;
    public const int PropGodmode2 = 22;

    public const int CfFrozen = 1 << 1;
    public const int CfNotarget = 1 << 2;
    public const int CfInstantWeapSwitch = 1 << 3;
    public const int CfTotallyFrozen = 1 << 5;
    public const int CfFrightening = 1 << 10;
    public const int CfNoclip = 1 << 11;
    public const int CfNoclip2 = 1 << 12;
    public const int CfGodmode = 1 << 13;
    public const int CfGodmode2 = 1 << 14;
    public const int CfBuddha = 1 << 15;
    public const int CfBuddha2 = 1 << 16;

    public const int Level2Allmap = 1;

    public static bool RequiresActivatorPlayer(bool whoIsActivator, object? activatorPlayer) =>
        whoIsActivator && activatorPlayer == null;

    public static bool IsPowerProperty(int which) =>
        which >= PropInvulnerability && which <= PropSpeed;

    public static int PowerIndex(int which) => which - PropInvulnerability;

    public static string? PowerNameAt(int index)
    {
        string?[] names =
        [
            "PowerInvulnerable",
            "PowerStrength",
            "PowerInvisibility",
            "PowerIronFeet",
            null,
            "PowerLightAmp",
            "PowerWeaponLevel2",
            "PowerFlight",
            null,
            null,
            "PowerSpeed",
            "PowerInfiniteAmmo",
            "PowerDoubleFiringSpeed",
            "PowerBuddha",
        ];
        return index >= 0 && index < names.Length ? names[index] : null;
    }

    public static bool PowerEntryValid(int powerIndex) =>
        PowerNameAt(powerIndex) != null || powerIndex == 4;

    public static bool IsAllMapPower(int powerIndex) => powerIndex == 4;

    public static bool ShouldApplyInvulnBlend(int powerIndex, int value) =>
        powerIndex == 0 && value == 1;

    public static int CheatMaskForProperty(int which) => which switch
    {
        PropBuddha => CfBuddha,
        PropBuddha2 => CfBuddha2,
        PropFrozen => CfFrozen,
        PropNotarget => CfNotarget,
        PropInstantWeaponSwitch => CfInstantWeapSwitch,
        PropTotallyFrozen => CfTotallyFrozen,
        PropFrightening => CfFrightening,
        PropNoclip => CfNoclip,
        PropNoclip2 => CfNoclip | CfNoclip2,
        PropGodmode => CfGodmode,
        PropGodmode2 => CfGodmode2,
        _ => 0,
    };

    public static bool IsFlyProperty(int which) => which == PropFly;

    public static int ApplyCheats(int cheats, int mask, bool set) =>
        set ? cheats | mask : cheats & ~mask;

    public static int FrozenCompatMask(int mask, bool compatLinkFrozen) =>
        compatLinkFrozen && (mask & (CfFrozen | CfTotallyFrozen)) != 0
            ? CfFrozen | CfTotallyFrozen
            : mask;

    public static int LevelAllmapSet(int flags2, bool set) =>
        set ? flags2 | Level2Allmap : flags2 & ~Level2Allmap;

    public static bool PropertyReturnsSuccess(int mask) => mask != 0;

    public static bool ApplyToAllPlayers(int who) => who != 0;
}
