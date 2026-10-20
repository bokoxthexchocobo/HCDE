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

    public static bool GivePowerToActivator(int who, int value) => who == 0 && value != 0;

    public static bool TakePowerFromActivator(int who, int value) => who == 0 && value == 0;

    public static bool ConsolePlayerAllmap(object? player, object? consolePlayer) =>
        ReferenceEquals(player, consolePlayer);

    public static int FlyCheatFlagsOn() => 1 | 2 | 4;

    public static int FlyCheatFlagsOff() => 0;

    public static bool ShouldSkipFlyMask(int which) => which == PropFly;

    public static bool ReturnsFalseWhenNoMask(int mask, int which) =>
        !ShouldSkipFlyMask(which) && mask == 0;

    public readonly record struct PowerGrantPlan(int Who, int Value, int Which, bool IsGive);

    public static PowerGrantPlan BuildPowerPlan(int who, int value, int which) =>
        new(who, value, which, value != 0);

    public static bool ShouldGrantAllMapToConsole(PowerGrantPlan plan, bool isAllMap, bool isConsole) =>
        plan.IsGive && isAllMap && isConsole;

    public static bool ShouldRevokeAllMapFromConsole(PowerGrantPlan plan, bool isAllMap, bool isConsole) =>
        !plan.IsGive && isAllMap && isConsole;

    public static int MergeCheatsForFly(int cheats, bool enable) =>
        enable ? cheats | FlyCheatFlagsOn() : cheats & ~FlyCheatFlagsOn();

    public static bool PlayerInGameForProperty(bool inGame, object? mo) =>
        inGame && mo != null;

    public static int SelectPowerMask(int which, int value) =>
        IsPowerProperty(which) ? 0 : CheatMaskForProperty(which);

    public static bool PropertySuccess(int which, int mask) =>
        IsPowerProperty(which) || (!ShouldSkipFlyMask(which) && mask != 0);

    public static IEnumerable<int> ActivePlayerSlots(bool[] inGame)
    {
        for (var i = 0; i < inGame.Length; i++)
        {
            if (inGame[i])
                yield return i;
        }
    }

    public static bool ShouldApplyPowerToPlayer(PowerGrantPlan plan, bool inGame, object? mo) =>
        plan.Who != 0 ? PlayerInGameForProperty(inGame, mo) : inGame;

    public static bool PowerLoopShouldContinue(int powerIndex) => PowerEntryValid(powerIndex) || IsAllMapPower(powerIndex);

    public static int ApplyFlagPropertyToCheats(int cheats, int which, int value, bool compatFrozen) =>
        ApplyCheats(cheats, FrozenCompatMask(CheatMaskForProperty(which), compatFrozen), value != 0);

    public static bool FlyPropertySetsMovement(int which, bool enable) =>
        IsFlyProperty(which) && enable;

    public static bool FlyPropertyClearsMovement(int which, bool enable) =>
        IsFlyProperty(which) && !enable;

    public static int ActivatorCheatsAfterProperty(int cheats, int which, int value, bool compatFrozen, bool enableFly) =>
        IsFlyProperty(which)
            ? MergeCheatsForFly(cheats, value != 0)
            : ApplyFlagPropertyToCheats(cheats, which, value, compatFrozen);

    public static bool RejectWhenNoActivatorAndWhoZero(object? player, int who) =>
        RequiresActivatorPlayer(who == 0, player);

    public static int AllmapFlags2Update(int flags2, PowerGrantPlan plan, bool console) =>
        ShouldGrantAllMapToConsole(plan, true, console)
            ? LevelAllmapSet(flags2, true)
            : ShouldRevokeAllMapFromConsole(plan, true, console)
                ? LevelAllmapSet(flags2, false)
                : flags2;

    public readonly record struct FlagPropertyPlan(int Who, int Value, int Which);

    public static FlagPropertyPlan BuildFlagPlan(int who, int value, int which) => new(who, value, which);

    public static bool FlagPlanTargetsActivator(FlagPropertyPlan plan) => plan.Who == 0;

    public static bool FlagPlanTargetsEveryone(FlagPropertyPlan plan) => plan.Who != 0;

    public static int ResolveCheatMask(FlagPropertyPlan plan) => CheatMaskForProperty(plan.Which);

    public static bool ShouldReturnEarly(FlagPropertyPlan plan, object? activatorPlayer) =>
        RejectWhenNoActivatorAndWhoZero(activatorPlayer, plan.Who);

    public static int ApplyToActivatorCheats(int cheats, FlagPropertyPlan plan, bool compatFrozen) =>
        ActivatorCheatsAfterProperty(cheats, plan.Which, plan.Value, compatFrozen, plan.Value != 0);

    public static int ApplyToPlayerCheats(int cheats, FlagPropertyPlan plan, bool compatFrozen) =>
        ApplyFlagPropertyToCheats(cheats, plan.Which, plan.Value, compatFrozen);

    public static bool PowerPlanTargetsEveryone(PowerGrantPlan plan) => plan.Who != 0;

    public static string? ResolvePowerName(PowerGrantPlan plan) =>
        IsPowerProperty(plan.Which) ? PowerNameAt(PowerIndex(plan.Which)) : null;

    public static bool InvulnerabilityBlendApplies(PowerGrantPlan plan) =>
        ShouldApplyInvulnBlend(PowerIndex(plan.Which), plan.Value);

    public static bool PowerGrantUsesInventory(PowerGrantPlan plan) =>
        IsPowerProperty(plan.Which) && !IsAllMapPower(PowerIndex(plan.Which));

    public static bool PowerRevokeUsesDestroy(PowerGrantPlan plan) =>
        IsPowerProperty(plan.Which) && !plan.IsGive && !IsAllMapPower(PowerIndex(plan.Which));

    public static bool GodmodeMaskSelected(int which) =>
        which == PropGodmode || which == PropGodmode2;

    public static bool BuddhaMaskSelected(int which) =>
        which == PropBuddha || which == PropBuddha2;
}
