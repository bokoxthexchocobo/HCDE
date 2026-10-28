namespace HCDE.Playsim;

/// <summary>
/// <c>LS_Thing_Hate</c> targeting from <c>p_lnspec.cpp</c> (**1565–1714**); chunk **943** (pivot: EOF at **3982**, **3983–4132** absent).
/// </summary>
public static class VmPLnspecThingHate943Slice
{
    public const int MfShootable = 1 << 0;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf3NoSightCheck = 1 << 2;
    public const int Mf3HuntPlayers = 1 << 3;
    public const int Mf4NoHatePlayers = 1 << 4;

    public static bool HateeCandidateValid(int flags, int health, int flags2) =>
        (flags & MfShootable) != 0 && health > 0 && (flags2 & Mf2Dormant) == 0;

    public static bool NothingToHateWhenTidNonZero(object? hatee, int hateeTid) =>
        hateeTid != 0 && hatee is null;

    public static bool RejectPlayerAsHater(bool activatorIsPlayer, int haterTid) =>
        haterTid == 0 && activatorIsPlayer;

    public static bool HaterIteratorCandidate(int health, int flags) =>
        health > 0 && (flags & MfShootable) != 0;

    public static bool UsesGroupHateMode(int arg2) => arg2 != 0;

    public static bool ShouldRecordTidToHate(int arg2, int hateeTid) => arg2 != 0;

    public static bool ShouldClearTargetForTid(int hateeTid, int targetTid) =>
        hateeTid != 0 && targetTid != hateeTid;

    public static int ApplyNoSightCheckFlag(int flags3, int arg2) =>
        arg2 is 2 or 4 or 6 ? flags3 | Mf3NoSightCheck : flags3 & ~Mf3NoSightCheck;

    public static int ApplyHuntPlayersFlag(int flags3, int arg2) =>
        arg2 is 3 or 4 ? flags3 | Mf3HuntPlayers : flags3 & ~Mf3HuntPlayers;

    public static int ApplyNoHatePlayersFlag(int flags4, int arg2) =>
        arg2 is 5 or 6 ? flags4 | Mf4NoHatePlayers : flags4 & ~Mf4NoHatePlayers;

    public static bool AssignActivatorAsHatee(int hateeTid) => hateeTid == 0;

    public static bool SkipSelfReference(object? hater, object? hatee) => ReferenceEquals(hater, hatee);

    public static bool CanChaseAfterHate(object? hatee, object? hater, object? goal, object? target, int arg2) =>
        hatee != null
        && !ReferenceEquals(hatee, hater)
        && (arg2 == 0 || (goal != null && !ReferenceEquals(target, goal)));

    public static bool ShouldWakeSeeState(int flags2, int health) =>
        (flags2 & Mf2Dormant) == 0 && health > 0;

    public static bool ContinueHaterIterator(int haterTid) => haterTid != 0;

    public readonly record struct HateAttitude(int Flags3, int Flags4);

    public static HateAttitude BuildHateAttitude(int flags3, int flags4, int arg2)
    {
        var nextFlags3 = ApplyHuntPlayersFlag(ApplyNoSightCheckFlag(flags3, arg2), arg2);
        return new(nextFlags3, ApplyNoHatePlayersFlag(flags4, arg2));
    }

    public static bool HateeScanContinue(object? hatee, object? hater, int flags, int health, int flags2) =>
        hatee is null
        || ReferenceEquals(hatee, hater)
        || (flags & MfShootable) == 0
        || health <= 0
        || (flags2 & Mf2Dormant) != 0;

    public static bool ShouldStoreLastEnemyBeforeRetarget(object? currentTarget) => currentTarget is not null;

    public static bool HasSeeStateForHate(object? seeState) => seeState is not null;

    public static bool ShouldBreakHaterLoop(int haterTid, object? nextHater)
    {
        if (haterTid == 0)
            return true;
        return nextHater is null;
    }
}
