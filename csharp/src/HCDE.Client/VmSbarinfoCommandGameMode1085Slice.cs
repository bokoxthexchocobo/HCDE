namespace HCDE.Client;

/// <summary>
/// <c>CommandGameMode</c>, ammo/inventory flow control, and <c>CommandAspectRatio</c> parse head from
/// <c>sbarinfo_commands.cpp</c> (**1801–1950**). Chunk **1085**; follows **1084** <c>CommandGameMode</c> ctor (**1800**).
/// </summary>
public static class VmSbarinfoCommandGameMode1085Slice
{
    public enum GameModeFlag
    {
        SinglePlayer = 0x1,
        Cooperative = 0x2,
        DeathMatch = 0x4,
        TeamGame = 0x8
    }

    public static (bool Recognized, int ModeIndex) TryResolveGameModeKeyword(string identifier)
    {
        if (identifier.Equals("singleplayer", StringComparison.OrdinalIgnoreCase))
            return (true, 0);
        if (identifier.Equals("cooperative", StringComparison.OrdinalIgnoreCase))
            return (true, 1);
        if (identifier.Equals("deathmatch", StringComparison.OrdinalIgnoreCase))
            return (true, 2);
        if (identifier.Equals("teamgame", StringComparison.OrdinalIgnoreCase))
            return (true, 3);
        return (false, -1);
    }

    public static int MergeGameModeKeywordIntoMask(int modes, int modeIndex) =>
        modeIndex < 0
            ? modes
            : VmSbarinfoCommandGameMode1084Slice.ApplyCommandGameModeFlagBit(modes, 1 << modeIndex);

    public static (bool EmitWarning, bool NextWarnUnknown) ResolveUnknownGameModeWarningState(
        bool recognizedKeyword,
        bool warnUnknownStillActive)
    {
        if (recognizedKeyword || !warnUnknownStillActive)
            return (false, warnUnknownStillActive);
        return (true, false);
    }

    public static bool ResolveGameModeFlowControlTruth(
        int modes,
        bool multiplayer,
        bool deathmatch,
        bool teamplay)
    {
        if (!multiplayer && (modes & (int)GameModeFlag.SinglePlayer) != 0)
            return true;
        if (deathmatch && (modes & (int)GameModeFlag.DeathMatch) != 0)
            return true;
        if (multiplayer && !deathmatch && (modes & (int)GameModeFlag.Cooperative) != 0)
            return true;
        if (teamplay && (modes & (int)GameModeFlag.TeamGame) != 0)
            return true;
        return false;
    }
}

public static class VmSbarinfoCommandUsesAmmo1085Slice
{
    public static bool ResolveUsesAmmoFlowControlTruth(bool hasPrimaryAmmo, bool hasSecondaryAmmo) =>
        hasPrimaryAmmo || hasSecondaryAmmo;

    public static bool ResolveUsesSecondaryAmmoFlowControlTruth(
        bool hasSecondaryAmmo,
        bool secondarySameAsPrimary) =>
        hasSecondaryAmmo && !secondarySameAsPrimary;
}

public static class VmSbarinfoCommandInventoryBar1085Slice
{
    public static bool ResolveInventoryBarNotVisibleTruth(int inventoryTics, bool levelDisablesInventoryBar) =>
        inventoryTics <= 0 || levelDisablesInventoryBar;
}

public static class VmSbarinfoCommandAspectRatio1085Slice
{
    public enum AspectRatioKind
    {
        Ratio4_3 = 0,
        Ratio16_9 = 1,
        Ratio16_10 = 2,
        Ratio17_10 = 3,
        Ratio5_4 = 4
    }

    public const int AspectRatioMapLength = 5;

    public static (bool Recognized, AspectRatioKind Kind) TryParseAspectRatioKeyword(string token)
    {
        if (token.Equals("4:3", StringComparison.OrdinalIgnoreCase))
            return (true, AspectRatioKind.Ratio4_3);
        if (token.Equals("16:9", StringComparison.OrdinalIgnoreCase))
            return (true, AspectRatioKind.Ratio16_9);
        if (token.Equals("16:10", StringComparison.OrdinalIgnoreCase))
            return (true, AspectRatioKind.Ratio16_10);
        if (token.Equals("17:10", StringComparison.OrdinalIgnoreCase))
            return (true, AspectRatioKind.Ratio17_10);
        if (token.Equals("5:4", StringComparison.OrdinalIgnoreCase))
            return (true, AspectRatioKind.Ratio5_4);
        return (false, AspectRatioKind.Ratio4_3);
    }

    public static int RegisterAspectRatioSelfMapping(int[] ratioMap, AspectRatioKind configuredRatio)
    {
        var index = (int)configuredRatio;
        ratioMap[index] = index;
        return index;
    }

    public static bool ResolveAspectRatioFlowControlTruth(int mappedActiveRatio, int configuredRatio) =>
        mappedActiveRatio == configuredRatio;
}
