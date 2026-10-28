namespace HCDE.Playsim;

/// <summary>
/// Weapon psprite constants, generic button routing table, and <c>DPSprite</c> field registration head from
/// <c>p_pspr.cpp</c> (**1–150**). Exclusive to chunk **967**.
/// </summary>
public static class VmPPsprHead967Slice
{
    public const double Lowerspeed = 6.0;

    public const int WrfNoBob = 1;
    public const int WrfNoSwitch = 1 << 1;
    public const int WrfNoPrimary = 1 << 2;
    public const int WrfNoSecondary = 1 << 3;
    public const int WrfNoFire = WrfNoPrimary | WrfNoSecondary;
    public const int WrfAllowReload = 1 << 4;
    public const int WrfAllowZoom = 1 << 5;
    public const int WrfDisableSwitch = 1 << 6;
    public const int WrfAllowUser1 = 1 << 7;
    public const int WrfAllowUser2 = 1 << 8;
    public const int WrfAllowUser3 = 1 << 9;
    public const int WrfAllowUser4 = 1 << 10;

    public readonly record struct GenericButtonRow(int ReadyFlag, int StateFlag, int ButtonFlag, string StateName);

    public static IReadOnlyList<GenericButtonRow> ButtonChecks { get; } = new GenericButtonRow[]
    {
        new(WrfAllowZoom, WeaponFlagZoomOk, ButtonZoom, "Zoom"),
        new(WrfAllowReload, WeaponFlagReloadOk, ButtonReload, "Reload"),
        new(WrfAllowUser1, WeaponFlagUser1Ok, ButtonUser1, "User1"),
        new(WrfAllowUser2, WeaponFlagUser2Ok, ButtonUser2, "User2"),
        new(WrfAllowUser3, WeaponFlagUser3Ok, ButtonUser3, "User3"),
        new(WrfAllowUser4, WeaponFlagUser4Ok, ButtonUser4, "User4"),
    };

    public const int WeaponFlagZoomOk = 1 << 6;
    public const int WeaponFlagReloadOk = 1 << 5;
    public const int WeaponFlagUser1Ok = 1 << 8;
    public const int WeaponFlagUser2Ok = 1 << 9;
    public const int WeaponFlagUser3Ok = 1 << 10;
    public const int WeaponFlagUser4Ok = 1 << 11;

    public const int ButtonReload = 1 << 6;
    public const int ButtonZoom = 1 << 7;
    public const int ButtonUser1 = 1 << 21;
    public const int ButtonUser2 = 1 << 22;
    public const int ButtonUser3 = 1 << 23;
    public const int ButtonUser4 = 1 << 24;

    public const int SvFastWeaponsServerInfo = 1;

    public static GenericButtonRow? FindButtonRowByReadyFlag(int readyFlag)
    {
        foreach (var row in ButtonChecks)
        {
            if (row.ReadyFlag == readyFlag)
                return row;
        }

        return null;
    }

    public static bool ReadyFlagAllowsButton(int weaponStateFlags, GenericButtonRow row) =>
        (weaponStateFlags & row.StateFlag) != 0;

    public static bool InputPressedForRow(int buttons, GenericButtonRow row) =>
        (buttons & row.ButtonFlag) != 0;

    public static IReadOnlyList<string> DpspritePointerFields { get; } = new[] { "Caller", "Next" };

    public static IReadOnlyList<string> DpspriteScalarFields { get; } = new[]
    {
        "CurState", "Caller", "Next", "Owner", "Sprite", "Frame", "Flags", "ID", "processPending",
        "x", "y", "oldx", "oldy", "baseScale", "pivot", "scale", "rotation",
    };

    public static IReadOnlyList<string> DpspriteFlagBitNames { get; } = new[]
    {
        "bAddWeapon", "bAddBob", "bPowDouble", "bCVarFast", "bFlip", "bMirror",
        "bPlayerTranslated", "bPivotPercent", "bInterpolate",
    };
}
