namespace HCDE.Playsim;

/// <summary>
/// <c>ChangeCamera</c> activator tail and <c>LS_SetPlayerProperty</c> power branch from <c>p_lnspec.cpp</c> (**2869–3018**); chunk **932**.
/// </summary>
public static class VmPLnspecCameraProperty932Slice
{
    public const int CfRevertPlease = 1;
    public const int Level2Allmap = 1;

    public const int PropInvulnerability = 5;
    public const int PropSpeed = 15;

    public static (object? camera, int cheats) AssignActivatorCamera(
        object? camera,
        object? activator,
        int cheats,
        int revertArg)
    {
        if (camera != null)
            return (camera, revertArg != 0 ? cheats | CfRevertPlease : cheats);
        return (activator, cheats & ~CfRevertPlease);
    }

    public static bool ShouldClearViewer(object? oldCamera, object? newCamera) =>
        !ReferenceEquals(oldCamera, newCamera);

    public static bool ShouldRejectPropertyCall(bool activatorHasPlayer, int who) =>
        !activatorHasPlayer && who == 0;

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

    public static bool IsGivePower(int value) => value != 0;

    public static bool ApplyToAllPlayers(int who) => who != 0;

    public static bool ShouldGrantAllMapToConsole(int which, int value, bool isConsole) =>
        IsGivePower(value) && IsAllMapPower(PowerIndex(which)) && isConsole;

    public static bool ShouldRevokeAllMapFromConsole(int which, int value, bool isConsole) =>
        !IsGivePower(value) && IsAllMapPower(PowerIndex(which)) && isConsole;

    public static int LevelAllmapSet(int flags2, bool set) =>
        set ? flags2 | Level2Allmap : flags2 & ~Level2Allmap;

    public static bool PowerGrantUsesInventory(int which, int value) =>
        IsPowerProperty(which) && !IsAllMapPower(PowerIndex(which)) && IsGivePower(value);

    public static bool PowerRevokeUsesDestroy(int which, int value) =>
        IsPowerProperty(which) && !IsGivePower(value) && !IsAllMapPower(PowerIndex(which));

    public static bool PlayerSlotActive(bool inGame, object? mo) => inGame && mo != null;

    public static int AllmapFlags2ForGrant(int flags2, int which, int value, bool isConsole)
    {
        if (ShouldGrantAllMapToConsole(which, value, isConsole))
            return LevelAllmapSet(flags2, true);
        if (ShouldRevokeAllMapFromConsole(which, value, isConsole))
            return LevelAllmapSet(flags2, false);
        return flags2;
    }

    public static bool InvulnerabilityBlendApplies(int which, int value) =>
        IsGivePower(value) && ShouldApplyInvulnBlend(PowerIndex(which), value);
}
