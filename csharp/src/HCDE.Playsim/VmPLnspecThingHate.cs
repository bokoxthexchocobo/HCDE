namespace HCDE.Playsim;

/// <summary>Native <c>LS_Thing_Hate</c> from <c>p_lnspec.cpp</c> (1565–1726).</summary>
public static class VmPLnspecThingHate
{
    public const int MfShootable = 1;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf3NoSightCheck = 1;
    public const int Mf3HuntPlayers = 1 << 1;
    public const int Mf4NoHatePlayers = 1;

    public static bool IsHateTargetValid(bool shootable, int health, bool dormant) =>
        shootable && health > 0 && !dormant;

    public static bool ShouldAbortWhenNoHateeFound(int hateeTid, object? hatee) =>
        hateeTid != 0 && hatee == null;

    public static bool ShouldRejectPlayerHater(bool activatorIsPlayer) => activatorIsPlayer;

    public static bool ShouldApplyGroupHate(int arg2) => arg2 != 0;

    public static bool ShouldClearTargetsForTid(int hateeTid) => hateeTid != 0;

    public static bool ShouldClearTargetEntry(int targetTid, int hateeTid) =>
        targetTid != hateeTid;

    public static int ApplySightCheckFlags(int flags3, int arg2) =>
        arg2 is 2 or 4 or 6 ? flags3 | Mf3NoSightCheck : flags3 & ~Mf3NoSightCheck;

    public static int ApplyHuntPlayersFlags(int flags3, int arg2) =>
        arg2 is 3 or 4 ? flags3 | Mf3HuntPlayers : flags3 & ~Mf3HuntPlayers;

    public static int ApplyNoHatePlayersFlags(int flags4, int arg2) =>
        arg2 is 5 or 6 ? flags4 | Mf4NoHatePlayers : flags4 & ~Mf4NoHatePlayers;

    public static bool ShouldAssignActivatorAsHatee(int hateeTid) => hateeTid == 0;

    public static bool ShouldSkipSelfHate(object? hater, object? hatee) => ReferenceEquals(hater, hatee);

    public static bool ShouldEnterSeeState(bool dormant, int health) => !dormant && health > 0;

    public static bool ShouldContinueIterator(int haterTid) => haterTid != 0;

    public static bool HasSeeState(object? seeState) => seeState != null;

    public static bool ShouldStoreLastEnemy(object? currentTarget) => currentTarget != null;

    public static bool ShouldAssignHateeWhenGoalDiffers(object? goal, object? target, object? hatee) =>
        hatee != null && !ReferenceEquals(hatee, goal) && (goal == null || !ReferenceEquals(target, goal));

    public readonly record struct HateIteratorState(
        int HateeTid,
        int Arg2,
        bool NothingToHate,
        object? ResolvedHatee);

    public static HateIteratorState BeginHate(int hateeTid, int arg2, object? firstHatee, bool valid) =>
        new(hateeTid, arg2, hateeTid != 0 && !valid, valid ? firstHatee : null);

    public static (int flags3, int flags4) ApplyArg2Attitude(int flags3, int flags4, int arg2) =>
        (ApplySightCheckFlags(flags3, arg2), ApplyNoHatePlayersFlags(ApplyHuntPlayersFlags(flags3, arg2), arg2));

    public static bool ShouldScanNextHateeInGroup(int arg2, bool nothingToHate) =>
        arg2 != 0 && !nothingToHate;

    public static bool IsEligibleGroupHatee(
        object? hatee,
        object? hater,
        bool shootable,
        int health,
        bool dormant) =>
        hatee != null
        && !ReferenceEquals(hatee, hater)
        && shootable
        && health > 0
        && !dormant;

    public static bool ShouldSetTarget(object? hatee, object? hater, int arg2, object? goal, object? target) =>
        hatee != null
        && !ReferenceEquals(hatee, hater)
        && (arg2 == 0 || (goal != null && !ReferenceEquals(target, goal)));

    public static object? SelectActivatorHatee(int hateeTid, object? activator) =>
        hateeTid == 0 ? activator : null;

    public static bool ResolveHaterHealthGate(int health, bool shootable) =>
        health > 0 && shootable;
}
